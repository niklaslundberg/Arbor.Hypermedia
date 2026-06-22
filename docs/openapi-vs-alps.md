# OpenAPI vs. ALPS — How They Differ Beyond URLs

> **Status:** Conceptual explainer (exploration in progress)
> **Date:** 2026-06-22
> **Audience:** Readers comfortable with OpenAPI/Swagger but new to ALPS.
> **Scope:** A neutral explanation of how OpenAPI and ALPS differ *conceptually*,
> setting aside the obvious point that OpenAPI enumerates URLs/paths and ALPS does
> not. Companion to [`html-hypermedia-apis.md`](./html-hypermedia-apis.md) §6.6,
> which takes a position on *using* these formats; this document only explains how
> they differ.

---

## 0. Why this document exists

The standard one-line summary — *"OpenAPI lists URLs, ALPS doesn't"* — is true but
shallow. It frames ALPS as "OpenAPI minus the paths," which badly misdescribes it.
The two formats are not the same kind of artifact with one feature toggled off.
They answer **different questions about an API**, and the URL difference is a
*symptom* of that deeper split, not the split itself.

This document explains the deeper differences, anchored against OpenAPI concepts a
reader already knows, with two axes treated in depth (**what each format
describes** and **validation vs. meaning**) and two treated more briefly (protocol
coupling and design-time vs. runtime).

---

## 1. The anchor: what OpenAPI is, restated precisely

To a reader who knows OpenAPI, it feels like "the description of my API." Stated
precisely, an OpenAPI document is an **interface description**: it enumerates the
concrete operations a server exposes and pins down the exact shape of the bytes
crossing the wire for each one.

Its primitives are:

- **Operations** — a `(path, HTTP method)` pair, e.g. `POST /orders`, each with an
  `operationId`.
- **Parameters** — path, query, header, and cookie inputs, each typed.
- **Request / response bodies** — described by **JSON Schema**: types, required
  fields, formats, enums, value constraints.
- **Status codes** — the enumerated outcomes of each operation.
- **Components** — reusable schemas, parameters, responses.

The mental model is a **contract over a call**: *"If you send exactly these bytes
to exactly this operation, you will get back exactly these bytes."* OpenAPI is, in
essence, a precise specification of message structure keyed by operation.

ALPS is not a thinner version of this. It is a description of something else
entirely. The next two sections are the heart of the comparison.

---

## 2. Axis 1 (in depth): what each format *describes*

This is the foundational difference; everything else follows from it.

### 2.1 OpenAPI describes an *interface* — the operations and their messages

OpenAPI's subject matter is the **concrete surface of one particular server**: the
operations it offers and the messages those operations exchange. It answers:

> *"What can I call, and what exactly do I send and receive?"*

Its descriptions are **operational and structural**. `POST /orders` with a body of
`{ ref: string, priority: enum, customer: uri }` returning `201` with an order
body is a statement about *mechanics* — which call, which fields, which shapes.

### 2.2 ALPS describes a *semantic profile* — the vocabulary and the transitions

ALPS — **Application-Level Profile Semantics** — has a different subject. It does
not describe a server's call surface. It describes the **vocabulary of meaning**
shared by a family of representations: *what concepts exist, what they mean, and
what state transitions are possible between them.* It answers:

> *"What do the things in this API mean, and what can one do, conceptually?"*

ALPS has essentially **one primitive — the descriptor** — used in two roles:

1. **Semantic descriptors (data)** — named concepts: `order`, `status`, `customer`,
   `dueDate`. A descriptor says *"there is a concept called `status` and here is
   what it means,"* and points (via `href`/`doc`) to a human- and/or
   machine-readable definition. It does **not** say what type `status` is, whether
   it is required, or how it is serialized.
2. **Transition descriptors (affordances)** — named possible state changes:
   `cancel-order`, `create-order`, with a `type` indicating the *nature* of the
   transition (`safe`, `idempotent`, `unsafe`) rather than an HTTP verb. A
   transition says *"`cancel-order` is a possible, unsafe state change relating
   these concepts,"* not *"send `DELETE` to this URL."*

Descriptors nest to express that, say, an `order` is described by a `ref`, a
`status`, a `customer`, and supports a `cancel-order` transition — all as
*meaning*, with no commitment to wire format.

### 2.3 The crux, stated as a contrast

| | **OpenAPI** | **ALPS** |
| --- | --- | --- |
| Subject | A specific server's call surface | A shared vocabulary of meaning |
| Core primitive | Operation (`path` + method) | Descriptor (concept or transition) |
| Answers | "What can I call & what bytes?" | "What do things mean & what's possible?" |
| Nature of statements | Operational / structural | Semantic / relational |
| Granularity of a transition | A concrete HTTP operation | An abstract, typed state change |

The line to hold onto: **OpenAPI describes an interface; ALPS describes a
vocabulary.** An interface is necessarily tied to *one implementation*; a
vocabulary can be *shared across many implementations* (different servers,
versions, even media types) that all mean the same things by the same descriptor
names. The absence of URLs in ALPS is downstream of this — a vocabulary of meaning
has no business naming one server's paths.

---

## 3. Axis 2 (in depth): validation vs. meaning

This is the difference most likely to surprise an OpenAPI user, because OpenAPI's
single most useful day-to-day feature is **precisely the thing ALPS deliberately
refuses to provide.**

### 3.1 OpenAPI is, at its heart, a validation contract

The payoff a team gets from OpenAPI is structural certainty:

- request/response bodies validated against **JSON Schema**;
- types, `required` fields, `format`s, `enum`s, `minimum`/`maxLength`/`pattern`
  constraints — all machine-checkable;
- code generators that emit typed clients/servers from those schemas;
- mock servers, contract tests, and request validators that *reject* a payload
  that does not conform.

OpenAPI tells you a `dueDate` is a `string` with `format: date`, that `priority`
must be one of `["standard","express"]`, and that `ref` is `required`. These are
**enforceable structural facts**, and most OpenAPI tooling exists to enforce them.

### 3.2 ALPS deliberately omits all of that

ALPS, by design, says **nothing** about structure or validation. From an ALPS
profile you cannot learn:

- the data **type** of `dueDate` (string? date? integer epoch?);
- whether `ref` is **required**;
- the **allowed values** of `priority`;
- how anything is **serialized** (JSON? XML? HTML form fields?);
- any **constraint** a payload must satisfy.

ALPS only tells you that the concepts `dueDate`, `ref`, and `priority` **exist and
what they mean.** It is a *semantic* document, not a *schema*. This is not an
oversight or an immaturity — it is the defining design decision. ALPS draws the
line at meaning and stops, on the principle that **meaning is stable while
structure is incidental**: the concept "the date an order is due" outlives any
particular decision to encode it as an ISO string vs. an epoch integer, or to
require it vs. default it.

### 3.3 Why split meaning from structure at all?

For an OpenAPI user this can feel like a downgrade — *"a description that can't
validate anything?"* The rationale is **reuse and longevity**:

- A **single** ALPS profile for "orders" can be honoured by a JSON API, an XML
  API, and an HTML hypermedia API simultaneously. Each chooses its own structure;
  all share the meaning. A JSON-Schema-bearing OpenAPI doc, by contrast, is welded
  to one structure.
- The **meaning** of `cancel-order` rarely changes; the *shape* of the cancel
  request changes often (new optional field, renamed property, different
  serialization). Putting only the durable part in the profile means the profile
  doesn't churn when the wire format does.
- It cleanly separates **two jobs**: *"what does this API mean"* (ALPS) from *"what
  bytes are valid right now"* (left to the media type / schema / the live
  representation itself).

### 3.4 The practical consequence

The two formats are therefore **not substitutes** on this axis — they are almost
complementary:

| | **OpenAPI** | **ALPS** |
| --- | --- | --- |
| Conveys data types | Yes (JSON Schema) | No |
| Conveys `required` / constraints / enums | Yes | No |
| Conveys serialization / media type | Yes (content types) | No (media-type agnostic) |
| Conveys *what a concept means* | Weakly (descriptions, prose) | **Yes — primary purpose** |
| Machine-validatable payloads | Yes | No |
| Survives a change of wire format | No (schema is the format) | Yes (meaning is format-free) |

A blunt way to put it: **OpenAPI knows the shape but not the point; ALPS knows the
point but not the shape.** OpenAPI can tell a generator how to build a valid
`POST /orders` body but leans on prose to say what an order *is*. ALPS can tell a
client what `order` and `cancel-order` *mean* but not one byte about how to
serialize them.

---

## 4. Worked example: one "orders" API, two descriptions

The contrast is easiest to feel on a single concrete API. Below is the *same*
orders domain — create an order, read it, cancel it — described first as an
OpenAPI fragment and then as an ALPS profile. Read them side by side and watch
what each one *can* and *cannot* say.

### 4.1 As OpenAPI (interface description)

```yaml
paths:
  /orders:
    post:
      operationId: createOrder
      requestBody:
        required: true
        content:
          application/json:
            schema:
              type: object
              required: [ref, customer]          # structure: what's mandatory
              properties:
                ref:      { type: string }        # structure: the type
                priority: { type: string, enum: [standard, express] }  # allowed values
                dueDate:  { type: string, format: date }                # serialization
                customer: { type: string, format: uri }
      responses:
        "201":
          description: Order created
          content:
            application/json:
              schema: { $ref: "#/components/schemas/Order" }
  /orders/{id}:
    get:
      operationId: getOrder
      parameters:
        - { name: id, in: path, required: true, schema: { type: string } }
      responses:
        "200": { description: The order, content: { application/json: { schema: { $ref: "#/components/schemas/Order" } } } }
    delete:
      operationId: cancelOrder            # "cancel" expressed as an HTTP DELETE on a URL
      parameters:
        - { name: id, in: path, required: true, schema: { type: string } }
      responses:
        "204": { description: Order cancelled }
```

Notice what this **pins down**: exact paths, HTTP methods, that `ref` is
`required`, that `priority` is one of two strings, that `dueDate` is a date-shaped
string, that bodies are `application/json`. It is fully **validatable**. Notice
what it **does not** say: what an order *means*, what cancelling *signifies*, or
that `cancelOrder` is "the cancel transition" rather than just "a DELETE."

### 4.2 As ALPS (semantic profile)

```xml
<alps version="1.0">
  <!-- data descriptors: concepts and their meaning -->
  <descriptor id="ref"      type="semantic" doc="Human reference for the order"/>
  <descriptor id="priority" type="semantic" doc="How urgently the order is handled"/>
  <descriptor id="dueDate"  type="semantic" doc="The date the order is due"/>
  <descriptor id="customer" type="semantic" doc="The customer the order is for"/>
  <descriptor id="status"   type="semantic" doc="Current lifecycle state of the order"/>

  <!-- the order concept, composed of the above + its possible transitions -->
  <descriptor id="order" type="semantic" doc="A customer's order">
    <descriptor href="#ref"/>
    <descriptor href="#priority"/>
    <descriptor href="#dueDate"/>
    <descriptor href="#customer"/>
    <descriptor href="#status"/>

    <!-- transition descriptors: possible state changes, typed by NATURE -->
    <descriptor id="cancel-order" type="unsafe" doc="Cancel this order"/>
  </descriptor>

  <descriptor id="create-order" type="unsafe" doc="Create a new order">
    <descriptor href="#ref"/>
    <descriptor href="#priority"/>
    <descriptor href="#dueDate"/>
    <descriptor href="#customer"/>
  </descriptor>
</alps>
```

Notice what this **pins down**: that the concepts `ref`, `priority`, `dueDate`,
`customer`, `status` exist and what they *mean*; that an `order` is composed of
them; that `create-order` and `cancel-order` are *possible transitions*, and that
both are `unsafe` (state-changing). Notice what it **deliberately omits**: no path,
no HTTP method, no `required`, no enum of priorities, no statement that `dueDate`
is a string vs. a date vs. an epoch, no media type. `cancel-order` is named and
*meant*, not bound to `DELETE /orders/{id}`.

### 4.3 Reading the two together

| What you want to know | OpenAPI tells you | ALPS tells you |
| --- | --- | --- |
| Which URL/method to call to cancel | `DELETE /orders/{id}` | — (out of scope) |
| Is `ref` required? | Yes | — |
| Allowed `priority` values | `standard`, `express` | — |
| How `dueDate` is serialized | date-formatted string | — |
| What an `order` *is* | (prose `description`, if any) | A first-class concept with named parts |
| What `cancel-order` *means* | (inferable from `DELETE`, prose) | A named, `unsafe` transition with `doc` |
| Could a JSON, XML, *and* HTML server share this? | No — schema is the format | Yes — meaning is format-free |

The same domain, described at two altitudes: OpenAPI nails the *bytes for one
server's calls*; ALPS nails the *vocabulary and transitions* and leaves the bytes
to whoever realizes the profile.

### 4.4 The same ALPS profile, realized two ways

The claim that one ALPS profile spans many implementations is abstract until you
see it. Here are **two different HTTP servers**, both honouring the *exact same*
§4.2 profile — same descriptors, same transition natures — while disagreeing on
every structural detail the profile left open.

**Realization A — a JSON REST server.** `create-order` (unsafe) is a `POST` of a
JSON body; `cancel-order` (unsafe) is a `DELETE` on the order resource:

```http
POST /orders                                    # create-order  (unsafe)
Content-Type: application/json

{ "ref": "PO-99", "priority": "express",
  "dueDate": "2026-07-01", "customer": "/customers/7" }

DELETE /orders/42                               # cancel-order  (unsafe)
```

**Realization B — an HTML hypermedia server.** The *same two transitions*, but
`create-order` is a form POST, and `cancel-order` is a `POST` to a cancellation
sub-resource using the method-override convention (cf. `html-hypermedia-apis.md`
§4.1):

```http
POST /orders                                    # create-order  (unsafe)
Content-Type: application/x-www-form-urlencoded

ref=PO-99&priority=express&dueDate=2026-07-01&customer=/customers/7

POST /orders/42/cancellation                    # cancel-order  (unsafe)
Content-Type: application/x-www-form-urlencoded

_method=DELETE
```

Both servers are **fully conformant to the one profile**, yet they share not a
single URL, verb, or serialization for `cancel-order`:

| Aspect (left open by ALPS) | Realization A | Realization B |
| --- | --- | --- |
| `cancel-order` URL | `/orders/42` | `/orders/42/cancellation` |
| `cancel-order` verb | `DELETE` | `POST` (+ `_method=DELETE`) |
| Body serialization | JSON | form-encoded |
| `dueDate` on the wire | JSON string | form field |
| What the profile fixed | `cancel-order` is an **unsafe** transition; `order` has `ref`/`priority`/`dueDate`/`customer`/`status` | *same* |

This is the payoff of §3 made concrete: because the profile asserts only *meaning*
(an `order` with these concepts; `cancel-order` as an unsafe transition), both
servers satisfy it without coordination. An OpenAPI document, by contrast, would
have had to *be* Realization A **or** Realization B — never both — because it commits
to the verb, URL, and schema that the two servers chose differently.

---

## 5. Axis 3 (brief): protocol & media-type coupling

OpenAPI is **bound to HTTP**. Its primitives *are* HTTP constructs — methods,
status codes, headers, paths, content types. You cannot describe an operation
without committing to an HTTP method and a media type. That binding is exactly why
it validates so well, and exactly why one OpenAPI document describes one HTTP
surface.

ALPS is **protocol- and media-type-agnostic.** A transition is typed as
`safe`/`idempotent`/`unsafe` — a *semantic* category — not as `GET`/`PUT`/`DELETE`.
The same ALPS profile is meant to be realizable over HTTP, over some other
protocol, in JSON, in XML, or in HTML. Protocol and format are treated as *binding
decisions made elsewhere* (in the actual representation), not part of the profile.

This is a direct consequence of Axes 1 and 2: a description of *meaning* has no
reason to name a protocol, just as it has no reason to name a URL or a JSON type.

---

## 6. Transition types vs. HTTP verbs

§5 said ALPS types a transition by its *nature* — `safe`, `idempotent`, `unsafe` —
rather than by an HTTP verb. That is worth unpacking, because it is the single
clearest illustration of how ALPS describes meaning while OpenAPI describes
mechanics, and it surprises OpenAPI users who expect a method.

### 6.1 ALPS reuses HTTP's *taxonomy* but not its *verbs*

The key observation: ALPS did not invent its three categories. **HTTP itself
classifies its methods** along exactly these lines — some methods are *safe* (no
intended side effects), some are *idempotent* (repeating has no further effect),
and the rest are neither. ALPS lifts that classification *up one level* and makes it
the primitive, then **drops the specific verbs.** Where HTTP says "`GET` is safe,
`PUT` and `DELETE` are idempotent, `POST` is neither," ALPS keeps the adjectives and
discards the nouns:

- **`safe`** — a read; observing state, no intended change.
- **`idempotent`** — a state change that can be repeated with the same result.
- **`unsafe`** — a state change that is not safely repeatable.

### 6.2 The mapping is deliberately one-to-many

Because ALPS keeps only the category, the relationship to verbs is **a fan-out, not
a lookup.** One ALPS type corresponds to *several* HTTP verbs, and ALPS refuses to
pick among them:

| ALPS type | HTTP verbs that honor it | What ALPS will *not* tell you |
| --- | --- | --- |
| `safe` | `GET` (also `HEAD`) | — (here it's effectively 1:1) |
| `idempotent` | `PUT`, `DELETE` | Whether this transition is a replace or a removal |
| `unsafe` | `POST`, `PATCH` | Whether it's an append/create or a partial update |

So in §4.2, `cancel-order` is typed `unsafe`. An HTTP server might realize that as
`DELETE /orders/{id}`, as `POST /orders/{id}/cancellation`, or as a `PATCH` setting
`status=cancelled`. An HTML server realizes it as a form submission. **All four are
valid honourings of the same `unsafe` transition,** and the profile is correct for
every one of them.

### 6.3 Why under-specify on purpose

This is the §3 "meaning vs. structure" split applied to the *action* rather than to
the *data*:

- The **durable, shareable fact** is the transition's *nature* — that cancelling an
  order is an unsafe state change. That is true regardless of which server, protocol,
  or verb realizes it.
- The **incidental, binding-time fact** is the verb — `DELETE` vs. `POST` vs. a form
  POST with `_method`. That is a decision each implementation makes, and it changes
  without the *meaning* of "cancel" changing.

Putting only the category in the profile keeps it honest across exactly the range of
realizations ALPS is built to span. An OpenAPI document, by contrast, *must* commit
to one verb — that is its job — which is precisely why an OpenAPI document describes
one server and an ALPS profile describes a family of them.

### 6.4 The edge that trips people up

`GET` is *both* safe and idempotent; `DELETE` and `PUT` are idempotent but not safe.
ALPS resolves the overlap by having a transition assert the **strongest** applicable
guarantee: a read is `safe` (not merely `idempotent`), because "safe" is the more
informative claim. And `PATCH`, which HTTP does *not* guarantee to be idempotent,
falls under `unsafe` rather than `idempotent` — ALPS classifies by the guarantee the
transition actually offers, not by the verb someone happens to bind it to.

---

## 7. Axis 4 (brief): design-time vs. runtime

OpenAPI is characteristically a **design-time / build-time artifact**: a file
checked into a repo, rendered in a developer portal, fed to code generators and CI
contract tests. It is *read about* the API, generally *before* or *outside* of
talking to it.

ALPS profiles are built to be **referenced from live representations at runtime** —
a representation points at its profile (e.g. via the `profile` link relation,
RFC 6906), and a client can dereference it to learn what the descriptors mean
*while interacting*. This pairs naturally with hypermedia, where what a client can
do next is discovered from the current representation rather than from a static map
read in advance.

(This axis is developed further, in the specific context of an HTML hypermedia
API, in `html-hypermedia-apis.md` §6.6. It is summarized here only to round out the
conceptual picture.)

---

## 8. How the profile attaches to a representation (RFC 6906)

§7 said ALPS profiles are *referenced from live representations at runtime.* That
reference is not hand-waving — it is a specific, standardized mechanism, and seeing
it closes the loop between "an abstract profile of meaning" and "the actual bytes a
client holds."

### 8.1 The gap the `profile` link fills

An ALPS profile (like §4.2) is a **standalone document**: it defines that
`cancel-order` is an unsafe transition and that `order` has a `status`, but it sits
at some URL of its own and is not, by itself, attached to anything. A representation
that *uses* those concepts needs a way to say: *"the descriptors you see in me are
defined over there."* That is exactly what **RFC 6906's `profile` link relation**
provides — a link whose target identifies *additional semantics* the representation
conforms to, without changing how the representation is otherwise parsed.

### 8.2 Two ways to attach it

The profile link rides on the same link machinery everything else does:

**As an HTTP `Link` header** (works for any media type, including JSON):

```http
HTTP/1.1 200 OK
Content-Type: application/json
Link: <https://example.com/alps/orders>; rel="profile"

{ "ref": "PO-99", "status": "open", ... }
```

**Inside the representation** (e.g. HTML's native `<link rel="profile">`):

```html
<head>
  <link rel="profile" href="https://example.com/alps/orders">
</head>
```

Either way the representation now points at the ALPS profile that gives its local
names and relations their meaning.

### 8.3 What a client does on dereference

The mechanism is **purely additive**, which is the important part:

- A client that **knows nothing** about the profile ignores the link and processes
  the representation exactly as before. Nothing breaks.
- A client that **cares about meaning** dereferences the profile URL once, reads the
  ALPS descriptors, and thereby learns what the representation's `status` field and
  `cancel-order` affordance *mean* — mapping the document's local tokens onto the
  shared, global semantic descriptors. It can then act on *meaning* (“find the
  unsafe `cancel-order` transition”) rather than on hard-coded field knowledge.

Because the profile is stable, a client **fetches it once and caches it**, reusing
that meaning across every representation that links to it — the profile is read at
runtime but not on every request.

### 8.4 Why this is the runtime counterpart to OpenAPI's design-time stance

This is the concrete embodiment of Axis 4 (§7). The meaning travels *with the
representation*, discoverable by following a link in the bytes the client already
holds. OpenAPI has **no equivalent move**: a JSON response does not carry a
`rel`-link to an OpenAPI operation that a client dereferences *at runtime* to
interpret the payload in hand. OpenAPI is consulted out-of-band, at design time, by
a developer — never followed live by the running client to decode the current
message.

And it ties §4.4 together: **both** Realization A (JSON/`DELETE`) and Realization B
(HTML/form) would emit `Link: <…/alps/orders>; rel="profile"` pointing at the *same*
profile URL. That shared profile link is precisely *how a client knows the two
structurally-different responses mean the same thing* — same meaning, advertised by
the same profile, honoured by different bytes.

---

## 9. Putting it together

The four axes are not independent — they are **one difference seen from four
angles**:

> OpenAPI is an **interface description**: a precise, structural, HTTP-bound,
> design-time contract over *one server's operations and message shapes*.
>
> ALPS is a **semantic profile**: an abstract, meaning-only, protocol-agnostic,
> runtime-referenceable vocabulary of *concepts and possible transitions* shared
> across implementations.

Every commonly cited difference falls out of that root contrast:

- *No URLs in ALPS* → because a vocabulary of meaning doesn't name one server's
  paths. **(The point this document set out to look past.)**
- *No types/validation in ALPS* → because meaning is separated from incidental
  structure (Axis 2).
- *No HTTP methods in ALPS* → because meaning is separated from protocol (Axis 3).
- *ALPS referenced at runtime* → because meaning is durable and worth dereferencing
  live (Axis 4).

They are best understood not as competitors but as descriptions **pitched at
different altitudes**: OpenAPI at the altitude of the wire, ALPS at the altitude of
the domain. Whether and how to use them *together* is a separate question — see the
companion report.

---

## 10. The misconception to retire: "ALPS is just OpenAPI minus the details"

Because this reader comes from OpenAPI, the most natural — and most misleading —
way to file ALPS away is as a **subset** of OpenAPI: *"take an OpenAPI doc, delete
the URLs, the methods, and the schemas, and what's left is ALPS."* This framing is
wrong in both directions, and it's worth seeing exactly why.

**It's not subtraction, it's a different axis.** If you literally strip the paths,
methods, and schemas out of an OpenAPI document, you do not get an ALPS profile —
you get a *broken OpenAPI document* with nothing left to say. What survives the
deletion (a list of `operationId`s and some prose) is not a semantic vocabulary; it
has no first-class `order` concept, no statement that `cancel-order` *means*
cancellation, no descriptors that other implementations could share. OpenAPI's
content is structural all the way down; remove the structure and there is no
residue of meaning, because meaning was never a first-class citizen in it.

**ALPS carries information OpenAPI lacks.** The subset framing implies you could
mechanically *add the details back* to an ALPS profile and recover OpenAPI. You
can't, and the reason is instructive: ALPS asserts things OpenAPI cannot express —
that a descriptor named `order` denotes one shared concept *across many
implementations, protocols, and media types.* That cross-implementation identity is
exactly what OpenAPI, welded to one server's surface, has no vocabulary for. ALPS
isn't OpenAPI with information removed; it's a description with *different*
information in it.

**Neither generates the other for free.** You cannot mechanically derive a useful
ALPS profile from an OpenAPI doc, because OpenAPI's "meaning" lives in optional,
unstructured `description` prose, not in machine-readable semantic descriptors. And
you cannot derive a working OpenAPI doc from ALPS, because ALPS deliberately
withholds every structural fact (types, required-ness, methods, paths) that
OpenAPI needs to validate a call. Each holds what the other drops.

The crisp correction: **OpenAPI and ALPS are not the same artifact at two levels of
detail. They are two different artifacts answering two different questions** — *how
do I call this server?* vs. *what does this API mean?* — that happen to overlap in
subject matter without overlapping in content.

---

## 11. Hydra as a foil — what ALPS's abstraction actually costs and buys

Holding ALPS up against a *third* format sharpens what is distinctive about it,
because Hydra sits in the gap between OpenAPI and ALPS and shows that ALPS's choices
were choices, not necessities.

**What Hydra is.** Hydra is a **JSON-LD vocabulary for hypermedia APIs.** Like ALPS,
it describes *meaning and transitions* — supported classes, their properties, and
the operations available on them — and it is **discoverable at runtime** (a
representation links to an `ApiDocumentation`, often via a `Link` header). On those
two counts it lines up with ALPS and against OpenAPI.

**Where Hydra diverges from ALPS.** Hydra does *not* share ALPS's radical
agnosticism. It carries structural and protocol commitments ALPS deliberately
refuses:

- It is **bound to JSON-LD / RDF.** A Hydra description *is* linked data; its
  classes and properties are RDF terms with IRIs. ALPS is serialization-neutral
  (JSON or XML, and indifferent to the representation's own media type).
- Its **operations carry an HTTP `method`** and `expects`/`returns` classes. So a
  Hydra operation says "this is a `POST` that expects an `Order`" — a *protocol and
  structural* commitment. ALPS's transition only says `unsafe`, naming the
  *nature*, not the verb (see §4.2).
- It therefore conveys **some structure** (which class a property belongs to, what
  an operation expects/returns), where ALPS conveys *only* meaning.

### 11.1 The three on one spectrum

Place the formats on a single axis from *pure meaning* to *concrete interface*:

| | **ALPS** | **Hydra** | **OpenAPI** |
| --- | --- | --- | --- |
| Describes meaning + transitions | Yes | Yes | Weakly (prose) |
| Runtime-discoverable | Yes | Yes | No (design-time) |
| Commits to a serialization | No | **Yes (JSON-LD)** | Yes (per content type) |
| Commits to a protocol/verb | No | **Yes (HTTP method)** | Yes (HTTP) |
| Carries structure (types/classes) | No | **Some (classes/props)** | Full (JSON Schema) |
| Reusable across media types | Yes | No (it *is* JSON-LD) | No |

The shape that emerges: **OpenAPI and ALPS are the two poles** — concrete interface
vs. pure meaning — and **Hydra sits in between, leaning to the hypermedia side.** It
keeps ALPS's runtime, meaning-and-transitions character but reintroduces the
serialization and protocol commitments that ALPS strips out.

### 11.2 What this reveals about ALPS

Hydra is the useful foil precisely because it proves a point about ALPS by
contrast: **you can be fully hypermedia-native and runtime-discoverable while still
carrying structure.** Hydra does exactly that. So ALPS's structure-free,
protocol-free, serialization-free character is **not a consequence of being a
hypermedia format** — it is a separate, deliberate decision to describe *meaning and
nothing else.*

That decision is the whole identity of ALPS. Its **cost** is everything cataloged in
§3.4: no validation, no types, no verb, nothing a generator can consume directly.
Its **buy** is maximal reuse and longevity: one profile honored by JSON, XML, and
HTML servers alike, untouched when any of them changes wire format. Hydra trades
some of that reuse (it is welded to JSON-LD) for the ability to carry structure and
operations. OpenAPI trades all of it for full, validatable structure. ALPS is the
format that refuses the trade — and Hydra, sitting halfway, is what makes that
refusal visible as a choice.

---

## 12. When each is the right tool (descriptively)

This explainer is neutral by design, so this coda matches *artifact to question*
rather than telling you which to adopt — the opinionated call, for the specific case
of an HTML hypermedia API, lives in the companion report (`html-hypermedia-apis.md`
§6.6). The fit follows directly from §2's "what each describes":

- **Reach for OpenAPI when the question is about a concrete call surface** — *"what
  endpoints exist, and what exact bytes do they take and return?"* Its commitment to
  URL, verb, and schema is exactly what powers typed client/server generation,
  payload contract-testing, mocking, and request validation. The thing that makes it
  implementation-specific is the thing that makes it *useful* for those jobs.

- **Reach for ALPS when the question is about durable, shareable meaning** — *"what
  do the concepts and transitions in this API mean, independent of how any one
  server wires them?"* It fits a vocabulary several implementations or media types
  must agree on, and a hypermedia API where clients discover meaning at runtime via a
  `profile` link (§8). Its silence on structure is the thing that lets one profile
  outlive and span many wire formats.

- **Reach for Hydra when you want ALPS's runtime, meaning-and-transitions character
  but are already in the JSON-LD / linked-data lane** and want structure carried
  along with the meaning (§11). It is the middle option when neither pole quite fits.

The three are **not mutually exclusive.** Because they answer different questions
(§9), a single team can legitimately maintain an ALPS profile as the durable
semantic contract *and* publish an OpenAPI document as a developer-onboarding view
of one server's concrete surface — each serving the audience and question it suits.
That they *can* coexist is a fact about their different subjects; whether a given
project *should* combine them, and how, is the recommendation left to the companion
report.

---

## 13. Open threads (to develop together)

The conceptual comparison feels complete. Remaining directions are optional
extensions rather than gaps:

- A fourth point of reference (JSON Hyper-Schema or Arazzo) if a wider survey is
  ever wanted — both were noted in `html-hypermedia-apis.md` §6.6 but are out of
  scope for a focused OpenAPI-vs-ALPS explainer.
- Splitting out the worked examples (§4, §8) into a runnable sample profile +
  representations, should this ever need to back a tutorial.
