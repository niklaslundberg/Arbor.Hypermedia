# HTML Hypermedia APIs — Design-Space Survey

> **Status:** Idea report / exploration
> **Date:** 2026-06-22
> **Scope:** A survey of how to build APIs whose representations are HTML and
> whose hypermedia controls *are* the API. Framework-agnostic; HTML snippets are
> illustrative, not tied to any particular server stack.

---

## 1. Premise

An *HTML hypermedia API* is an API whose responses are HTML documents, and where
the links and forms inside those documents are the complete, self-describing set
of things a client can do next. There is no separate JSON contract, no
out-of-band SDK that encodes URL templates, and no client that has memorised the
shape of the server. The client receives a representation of the current
application state, and the representation itself advertises every available state
transition.

This is HATEOAS taken literally: **HTML as the engine of application state.** The
same bytes that a browser renders for a human are the bytes a machine parses to
decide what to do next.

The interesting and difficult commitment in this report is that the API serves
**both humans in browsers and programmatic clients that parse the markup** — from
the *same* representation. That dual-consumer constraint is the lens through
which every design decision below is evaluated.

We explicitly **do not** compare HTML against JSON hypermedia formats (HAL,
Siren, JSON:API, Collection+JSON). HTML is taken as the chosen medium; the
question is how to do it well, not whether to do it.

---

## 2. Why this is worth surveying

HTML is the only hypermedia format with a universal, zero-install client already
on every device. That client (the browser) natively understands:

- **Navigation** — `<a href>` issues a GET and replaces application state.
- **Writes** — `<form>` collects input and submits a GET or POST.
- **Relationships** — `rel` attributes label what a link *means*.
- **Caching, redirects, auth, content negotiation** — all via HTTP, for free.

The catch is that browsers natively support only GET and POST, and only the
"replace the whole document" interaction model. The design space is largely about
(a) closing the gap between what HTTP offers and what HTML exposes, and (b)
making the markup parseable by machines without harming the human experience.

---

## 3. The consumer model: humans *and* machines from one representation

Designing for one audience is easy. Designing for both from identical bytes is
the crux, because their needs partially conflict:

| Concern | Human-in-browser | Machine parser |
| --- | --- | --- |
| Layout / styling | Wants it | Ignores it |
| Stable, semantic markers | Helpful | **Required** |
| Link relations | Mostly cosmetic | **Primary navigation signal** |
| Form labels | Read by people | Read as field semantics |
| Visual disambiguation | Position, color | Class/attribute only |

The reconciling principle: **presentation is layered on top of a stable semantic
skeleton.** The skeleton — which element carries which meaning, which `rel` a
link has, what a form's fields are named — is the contract. CSS, ordering, and
decorative wrappers are free to change. A machine client keys off the skeleton; a
human gets the styled view of the same skeleton.

Concretely, this means the report's later recommendations all bias toward
*marking meaning explicitly in attributes/classes* rather than relying on
document structure or visual position, because structure and position are the
parts most likely to churn for human-facing reasons.

---

## 4. The progressive-enhancement spectrum

The baseline is **pure HTML that works with JavaScript disabled.** Enhancement
with a generic, application-agnostic library (htmx, Unpoly, Turbo) is layered on
top — but no application-specific JavaScript. The API must remain fully usable if
the enhancement layer never loads.

### 4.1 Baseline: what raw browsers do

- Verbs: only **GET** and **POST** from forms.
- Interaction: full-page navigation/replacement.
- PUT / PATCH / DELETE: not directly expressible.

The standard accommodation is a **method-override convention**: the form submits
POST and carries the intended verb, which the server (or a middleware) maps back.

```html
<!-- A "delete" affordance that works in a plain browser -->
<form method="post" action="/orders/42" data-method="delete">
  <input type="hidden" name="_method" value="DELETE" />
  <button type="submit" rel="cancel-order">Cancel order</button>
</form>
```

Two things are encoded here deliberately:

1. `_method` — the no-JS fallback. The server reads it and treats the request as
   `DELETE`.
2. `data-method="delete"` — a hook the enhancement layer can read to issue a real
   `DELETE` (and, for a machine client, an unambiguous statement of the true
   verb).

### 4.2 Enhanced: generic hypermedia libraries

htmx-style libraries extend HTML's vocabulary so that *any element* can issue
*any HTTP verb* and swap *a fragment* of the page rather than the whole document.
Critically, they do this with declarative attributes, so the markup stays
hypermedia — the affordances are still in the response, not in hand-written JS.

```html
<!-- Same affordance, progressively enhanced -->
<form method="post" action="/orders/42"
      data-method="delete"
      hx-delete="/orders/42" hx-target="#order-42" hx-swap="outerHTML">
  <input type="hidden" name="_method" value="DELETE" />
  <button type="submit">Cancel order</button>
</form>
```

If htmx loads, the real `DELETE` fires and only `#order-42` is replaced. If it
doesn't, the form POSTs with `_method=DELETE` and the page reloads. **Same
affordance, two fidelities.**

### 4.3 Design implications of the spectrum

- The **server must accept both** the override-POST and the native verb for every
  write. The override is not a hack to be removed later; it is the floor of the
  contract.
- Enhancement attributes (`hx-*`) should be **derivable from the semantic
  skeleton**, not a parallel source of truth. If the form already declares its
  action, verb, and target relation, the `hx-*` attributes are a mechanical
  projection of those facts. This keeps the two fidelities from drifting.
- Fragment responses (partial swaps) must themselves be **valid standalone
  hypermedia** — a returned fragment should carry the same rels and semantics as
  the equivalent region of a full page, so a machine client gets identical
  meaning whether it fetched a page or a fragment.

---

## 5. Encoding semantics for machine consumers

A machine needs to find, in the markup, *which element is the entity*, *what type
it is*, *which span is which property*, and *which control is which affordance*.
Three families of techniques compete. This section compares them and recommends.

### 5.1 Option A — Microformats2 (class-based conventions)

Root classes prefixed `h-` mark typed objects; `p-`, `u-`, `dt-`, `e-` prefixes
mark plain-text, URL, datetime, and embedded-markup properties.

```html
<article class="h-order" id="order-42">
  <a class="u-url" rel="self" href="/orders/42">Order #42</a>
  <span class="p-status">shipped</span>
  <time class="dt-placed" datetime="2026-06-01T10:30:00Z">June 1</time>
</article>
```

- **Pros:** no extra vocabulary files; uses ordinary `class`, so it's invisible to
  humans and stylable; tiny conceptual surface; well-supported parsers exist; the
  `h-`/`p-`/`u-`/`dt-` prefixes double as a naming discipline.
- **Cons:** vocabulary is a flat, community-curated set; expressing deep or novel
  domain graphs means inventing your own `h-*` types that no generic parser knows;
  no formal schema or validation; relationships between nested objects are
  implied by DOM nesting rather than stated explicitly.

### 5.2 Option B — RDFa / Microdata (inline structured data + vocabularies)

Attributes (`typeof`/`property`/`resource` for RDFa, `itemscope`/`itemtype`/
`itemprop` for Microdata) bind markup to a vocabulary such as schema.org, yielding
a real graph.

```html
<article typeof="schema:Order" resource="/orders/42">
  <a property="schema:url" rel="self" href="/orders/42">Order #42</a>
  <span property="schema:orderStatus">shipped</span>
  <time property="schema:orderDate" datetime="2026-06-01T10:30:00Z">June 1</time>
</article>
```

- **Pros:** standardized, extensible vocabularies; expresses arbitrary graphs with
  explicit subject/predicate/object; benefits from existing schema.org tooling and
  search-engine consumption; unambiguous identity via `resource`/IRIs.
- **Cons:** markup is noticeably more verbose; the mental model (RDF triples) is
  heavier than CSS classes; easy to get subtly wrong; some duplication between the
  vocabulary terms and human-readable labels.

### 5.3 Option C — Custom `data-*` attributes + link relations

Application-defined `data-*` attributes carry semantics; IANA-registered and
custom `rel` values carry relationships.

```html
<article data-entity="order" data-id="42" id="order-42">
  <a rel="self" href="/orders/42">Order #42</a>
  <span data-prop="status">shipped</span>
  <time data-prop="placed" datetime="2026-06-01T10:30:00Z">June 1</time>
</article>
```

- **Pros:** maximum control; `data-*` is the standards-blessed home for custom
  metadata and never collides with styling classes; trivially parseable with
  selectors; pairs naturally with rel-driven navigation.
- **Cons:** entirely bespoke — no generic parser understands your vocabulary, so
  clients must read your docs; you reinvent typing and validation; no
  interoperability dividend with the wider ecosystem.

### 5.4 Comparison and recommendation

| Criterion | Microformats2 | RDFa/Microdata | data-* + rels |
| --- | --- | --- | --- |
| Markup weight | Light | Heavy | Light |
| Generic parser support | Good (common types) | Good (schema.org) | None (bespoke) |
| Expresses deep graphs | Weak | Strong | Manual |
| Collides with styling? | Shares `class` | No | No |
| Learning curve for client | Low | Medium | Low (but read docs) |
| Validation/schema | None | Vocabulary-backed | None |

**Recommendation for the dual-consumer goal:** adopt a **layered convention with
a clear primary**:

1. **Use `data-*` attributes as the load-bearing, normative machine contract** for
   entity identity, type, and property names. They are unambiguous, collision-free
   with CSS, and fully under your control — which matters most for a *typed,
   rel-driven* API where the client follows your published vocabulary anyway.
2. **Layer Microformats2 `h-*`/`p-*` classes on top** for the common, generic
   types (people, events, dates, URLs). This buys "for free" interoperability with
   off-the-shelf parsers and a useful naming discipline, without committing your
   whole domain to a flat vocabulary.
3. **Reach for RDFa/schema.org selectively**, only on resources where
   search-engine visibility or genuine graph interchange justifies the verbosity.

The throughline: *one normative channel you own* (`data-*` + rels) so machine
clients have a single, stable place to look, plus *opportunistic standard
markup* where the ecosystem rewards it. Avoid encoding the same fact three times
as the canonical source — pick `data-*` as canonical and treat the others as
additive enhancement, exactly as JS enhancement is additive over baseline HTML.

---

## 6. Affordances: typed link relations everywhere

The organizing rule for this API: **every link and every form carries a link
relation from a documented vocabulary, and clients act on the relation — never on
the URL's structure.** URLs are opaque. The `rel` is the API.

### 6.1 Why rel-driven, not URL-driven

If a client hard-codes `/orders/{id}/cancel`, the server can never reorganize its
URL space without breaking clients — the very coupling hypermedia exists to
remove. If instead the client looks for `rel="cancel-order"` and submits whatever
form bears it, the server owns its URLs completely. This is the single most
important discipline for keeping an HTML API evolvable (see §8).

### 6.2 Two kinds of affordance

**Navigation links** — safe, idempotent transitions (GET). The relation says what
you'll get.

```html
<nav>
  <a rel="self"        href="/orders/42">This order</a>
  <a rel="collection"  href="/orders">All orders</a>
  <a rel="next"        href="/orders?page=3">Next page</a>
  <a rel="customer"    href="/customers/7">Customer</a>
</nav>
```

**Forms as the write/action layer** — unsafe or parameterized transitions. The
form *is* the affordance: it declares the verb, the target, the relation, and the
exact set of inputs the server expects, including their types and allowed values.
A machine client doesn't need an SDK to learn how to cancel an order — the form
tells it.

```html
<form method="post" action="/orders" data-method="post" rel="create-order">
  <fieldset>
    <legend>Create order</legend>

    <!-- text input: name is the field semantic, label is the human caption -->
    <label for="ref">Reference</label>
    <input type="text" name="ref" id="ref" required />

    <!-- single-selection: machine reads options, human sees a dropdown -->
    <label for="priority">Priority</label>
    <select name="priority" id="priority">
      <option value="standard">Standard</option>
      <option value="express">Express</option>
    </select>

    <!-- date affordance -->
    <label for="due">Due date</label>
    <input type="date" name="due" id="due" />

    <!-- reference to another resource, hidden from humans, meaningful to both -->
    <input type="hidden" name="customer" value="/customers/7" />

    <button type="submit">Create</button>
  </fieldset>
</form>
```

Note how the field *kinds* (text, single-selection, date, reference-to-resource)
map onto native HTML controls. That mapping is the entire reason HTML works as a
write hypermedia: the format already has a vocabulary of input affordances, and a
machine can enumerate `<input>`/`<select>` with their `name`, `type`, `required`,
and `option`/`value` sets to construct a valid request without prior knowledge.

### 6.3 Putting the relation on the right element

For machine clarity, the relation should sit on the element that *is* the
affordance:

- Navigation: `rel` on the `<a>`.
- Whole-form action: `rel` on the `<form>` (or, where a form hosts a single
  primary action, on its submit `<button>`).

A machine client's algorithm becomes uniform: *find elements bearing the rel I
want; if it's a link, GET its href; if it's a form, read its method/action/fields
and submit.*

### 6.4 The relation vocabulary itself

- **Prefer IANA-registered relations** (`self`, `next`, `prev`, `collection`,
  `edit`, `up`, `first`, `last`, …) wherever one fits. They're already understood.
- **Mint custom relations as URIs** (or CURIEs) for domain transitions
  (`cancel-order`, `create-order`), and make those URIs *dereferenceable to human-
  and machine-readable documentation*. A custom rel that resolves to a doc is how
  a client (or a developer) learns what the affordance does — this is the natural,
  lightweight alternative to a separate out-of-band profile.
- **Give every relation a stable identifier and a friendly display name.** The
  identifier is the contract (machine); the friendly name is the caption (human).
  These are two fields of one concept, and conflating them is a common mistake —
  the display name *will* change for copy/localisation reasons; the identifier
  must not.

---

## 7. Entities, nesting, and embedded vs. linked sub-resources

A recurring decision: when an order references a customer, do you **embed** a
representation of the customer inside the order's HTML, or **link** to it?

- **Link** keeps responses small and cache-friendly and avoids duplicating a
  resource's representation in many places. It costs the client a round trip.
- **Embed** lets a single response carry a useful sub-tree (e.g. the order's line
  items rendered inline) so the human sees a complete page and the machine gets
  the data in one fetch.

Guidance:

- Embed sub-resources that are **constituent** (line items of an order, fields of
  a profile) — they have no meaningful life outside the parent.
- Link sub-resources that are **independent** (the customer, the warehouse) and
  carry a typed `rel` to them.
- Make embedded sub-entities **self-identifying**: each should carry its own
  identity (`data-id`/`u-url`/`resource`) and a `rel="self"` link, so a machine
  can lift it out and treat it as a first-class resource. Nesting in the DOM
  should *imply* containment, but identity should be *stated*, not inferred from
  position (per §3).

```html
<article data-entity="order" data-id="42">
  <a rel="self" href="/orders/42">Order #42</a>

  <!-- embedded, but self-identifying, constituent sub-resource -->
  <section data-entity="line-item" data-id="42-1">
    <a rel="self" href="/orders/42/lines/1">Line 1</a>
    <span data-prop="sku">ABC-123</span>
    <span data-prop="qty">2</span>
  </section>

  <!-- independent resource: linked, not embedded -->
  <a rel="customer" href="/customers/7">Customer</a>
</article>
```

---

## 8. Contract evolution (core concern)

A hypermedia API's promise is that the server can change without breaking
clients. That promise is only kept if both sides honour an evolution discipline.
This is where most real HTML-API designs quietly fail.

### 8.1 What is the contract, and what is not

**Part of the contract (change carefully, additively):**

- Link relation *identifiers* and their documented meaning.
- Form field *names*, their `type`/control kind, and `required`-ness.
- The set of allowed values for selection fields (removing a value is breaking;
  adding one is usually safe).
- The normative machine markers (the canonical `data-*` channel from §5.4).

**Not part of the contract (change freely):**

- URL strings (clients follow rels, never construct URLs).
- DOM structure, element ordering, wrapper elements, CSS classes used only for
  styling, copy/friendly names, layout, the presence of the enhancement layer.

The single most valuable rule: **drive everything by relation and field name, so
that the entire visual/structural surface is free to evolve.**

### 8.2 Rules of additive change

- **Add, don't remove or repurpose.** New transition → new `rel`. New input →
  new optional field. Never change the meaning of an existing identifier.
- **New fields must be optional** (or carry a server default) for a grace period,
  so older clients that don't send them still succeed.
- **Deprecate, then retire.** Keep a retiring affordance present but marked (e.g.
  a documented `deprecation` link or attribute) for a window before removal, and
  announce removal through the rel's own documentation.
- **Removing an allowed value or making a field newly required is breaking** —
  treat it as a new affordance, not an edit to the old one.

### 8.3 How to write a robust client (the other half of the bargain)

Server discipline is wasted if clients are brittle. Robust hypermedia clients:

- **Find affordances by rel/field name**, never by position, CSS class, or URL
  pattern.
- **Tolerate unknowns.** Ignore elements, attributes, rels, and fields they don't
  recognize rather than failing. This is what lets the server add things freely.
- **Submit forms by reading them**, echoing back the fields the form declares
  (including hidden ones) rather than constructing requests from memory.
- **Re-fetch rather than cache transitions.** The set of available actions is a
  property of current state; a client should read it from the latest
  representation, not assume an order can still be cancelled because it could a
  minute ago.

### 8.4 Versioning posture

Prefer **no version in the URL.** Because clients follow rels and read forms, the
representation can evolve in place. If a genuinely breaking, non-additive change
is unavoidable, prefer introducing a **new relation / new affordance alongside
the old** over minting a `/v2/` URL space — the former lets a single client
straddle the transition by feature-detecting the rel, while the latter forks the
whole API. Reserve media-type or URL versioning for true epochs, not routine
change.

---

## 9. Cross-cutting concerns (brief)

- **Validation:** server-side validation is authoritative; re-render the form with
  error messages attached to the offending fields (associate errors with field
  names so machines can read them too). HTML5 constraints (`required`, `type`,
  `pattern`, `min`/`max`) are a usability and machine-hint bonus, never the
  enforcement boundary.
- **Caching & safety:** keep GET safe and idempotent so links are freely
  prefetchable; confine state change to POST/PUT/PATCH/DELETE. This keeps both
  browser caches and machine crawlers well-behaved.
- **Auth:** standard HTTP auth/session mechanisms apply unchanged; the affordances
  a representation exposes should reflect the caller's permissions (don't render a
  `cancel-order` form to a user who can't cancel — absence of an affordance is
  itself information).
- **Errors:** use real HTTP status codes; for human+machine parity, render an
  error body that is itself navigable hypermedia (a link back, a corrected form)
  rather than a dead end.
- **Testing:** because the contract is rels + form fields, tests should assert
  *"a `cancel-order` affordance with these fields is present in this state"*, not
  snapshot the HTML. This keeps the test suite aligned with the actual contract
  and free of presentational churn.

---

## 10. Open questions / risks

- **`data-*` vs Microformats2 canonical source (§5.4).** The layered recommendation
  must be enforced or the redundant encodings will drift. Is tooling needed to
  derive one from the other?
- **Custom-rel documentation hosting.** Dereferenceable rel URIs are great in
  theory; who owns that documentation surface and keeps it in sync with the code
  that emits the rels?
- **Fragment/partial responses (§4.3).** Ensuring a swapped fragment carries the
  same semantics as the full-page region is easy to forget; it may warrant a
  shared rendering path so a region is rendered identically standalone or embedded.
- **Selection-field value churn.** Allowed-value sets are part of the contract but
  feel like data; a governance answer is needed for who may remove a value.
- **Pagination and large collections.** `next`/`prev`/`first`/`last` rels cover
  the basics, but cursor stability and total-count exposure for machine clients
  need a convention.

---

## 11. Summary of recommendations

1. **HTML is the single representation**, serving humans and machines from the
   same bytes; presentation is layered over a stable semantic skeleton.
2. **Pure HTML is the floor**; the method-override convention (`_method` +
   `data-method`) makes PUT/PATCH/DELETE expressible without JS. A generic
   library (htmx-style) is an *additive* enhancement projected mechanically from
   the same skeleton — never a parallel contract.
3. **Encode machine semantics in a single canonical channel you own** (`data-*`
   attributes + rels), with Microformats2 `h-*` classes layered on for common
   generic types and RDFa/schema.org used selectively for interchange/SEO.
4. **Typed link relations everywhere.** Clients act on `rel`, never on URL
   structure. Navigation lives on links; actions live on forms that fully declare
   their verb, target, and typed fields.
5. **Mint custom rels as dereferenceable URIs** and separate the stable
   *identifier* from the changeable *friendly name*.
6. **Embed constituent sub-resources, link independent ones**, and make embedded
   entities self-identifying.
7. **Treat evolution as a first-class contract:** drive by rel + field name, change
   only additively, deprecate-then-retire, write tolerant clients, and avoid URL
   versioning in favour of additive affordances.
