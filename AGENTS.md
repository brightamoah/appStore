# Project Guidelines & Agent Instructions

## Vue & Nuxt Conventions

### Same-Name Prop Shorthand (Vue 3.4+)
Whenever a prop and the variable/ref being passed share the same name in Vue and Nuxt SFC templates, **always use the same-name prop shorthand** rather than the verbose syntax:

```vue
<!-- Recommended -->
<DashboardTasksPreview :tasks />
<DashboardProjects :projects />
<AuthAlert :error />

<!-- Avoid -->
<DashboardTasksPreview :tasks="tasks" />
<DashboardProjects :projects="projects" />
<AuthAlert :error="error" />
```

IDE inlay hints and Volar automatically indicate the bound variable, so keep template bindings concise, modern, and idiomatic.

### Icon Styling (No Backgrounds on Icons)
Never put a background container or background class on an icon (`UIcon`). Icons are standalone elements, so apply color classes directly to the icon itself (e.g. `text-primary`, `text-error`, `text-muted`), except when the icon is nested inside a badge (`UBadge`) where it naturally inherits the parent badge's theme color.

## C# / Backend & General Code Conventions

### Descriptive Naming (No Single-Letter or Cryptic Abbreviations)
Never use single letters or unnecessarily short names for variables, lambda parameters, or method arguments (e.g. avoid `p`, `c`, `t`, `ct`, `x`, `e`). Always use clear, self-documenting identifiers so that code is understandable at a glance:

```csharp
// Recommended
var projectExists = await context.Projects.AnyAsync(project => project.Id == projectId, cancellationToken);
var column = await context.BoardColumns.FirstOrDefaultAsync(column => column.Id == columnId, cancellationToken);

// Avoid
var projectExists = await context.Projects.AnyAsync(p => p.Id == projectId, ct);
var column = await context.BoardColumns.FirstOrDefaultAsync(c => c.Id == columnId, ct);