# Arbor.Hypermedia

Arbor.Hypermedia is a small ASP.NET Core library for serving HTML as hypermedia controls instead of static pages.  
The core idea is that each response describes:

- the current entity (`HyperMediaEntity`)
- its primitive properties (`ObjectControl`)
- related links (`HyperMediaLink`)
- available actions as forms (`HyperMediaForm`)

## Main ideas

- **Metadata-driven responses**: domain types implement `IMetadata` and return an `EntityMetadata` graph.
- **Hypermedia controls as first-class objects**: entities, links, forms, and fields are modeled in C# and rendered to HTML.
- **Route-based navigation**: URLs are resolved from named routes and route parameters through `IUrlResolver`.
- **Action discovery from metadata**: available commands are exposed as forms with correct HTTP methods and form fields.
- **Recursive composition**: entities can contain actions and related items, producing nested hypermedia documents.

## How it is implemented

1. **Metadata model**
   - `EntityMetadata` is the central structure with entity, route, method, actions, and items.
   - `IEntity` provides stable identity/context via `EntityContext`.
   - `IMetadata` lets any response model return its metadata tree.

2. **Control generation**
   - `HyperMediaBuilder` walks `EntityMetadata` and builds `IHyperMediaControl` objects.
   - For `GET` routes it emits a `self` link.
   - It reflects simple public properties and emits them as `ObjectControl`.
   - It turns action metadata into `HyperMediaForm` instances and recursively includes related entities.

3. **Rendering pipeline**
   - `HtmlHypermediaFormatter` is an MVC output formatter for `IMetadata` responses.
   - It resolves URLs, invokes `HyperMediaBuilder`, and renders embedded Razor views.
   - `Views/Shared/HyperMediaLayout.cshtml` and `HyperMediaView.cshtml` render entities, links, and forms.
   - Non-GET/POST actions are handled through method override (`_method`) in generated forms.

4. **Service registration**
   - `UseHypermedia()` registers MVC integration, embedded views, output formatter, and URL-encoded input formatter.
   - `HyperMediaBuilder` and MVC dependencies are wired through DI.

## Example flow (Todo sample/tests)

- Controllers return models such as `TodoList` or `TodoItem` that implement `IMetadata`.
- Each model returns metadata describing route names, actions (for example `mark done`, `comment`), and relations.
- The formatter converts that metadata into navigable HTML where the client can follow links and submit forms to move state forward.

## Source generator project

The repository also contains `Arbor.Hypermedia.Generators`, a Roslyn generator project with template-based code generation scaffolding for metadata-related code.
