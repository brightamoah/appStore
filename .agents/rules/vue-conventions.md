# Vue & Nuxt Conventions

## Same-Name Prop Shorthand (Vue 3.4+)
Whenever a prop and the variable/expression passed to it share the same name in Vue and Nuxt templates, **always use the same-name prop shorthand** rather than the verbose syntax.

### Examples:
- **Use:** `:tasks` instead of `:tasks="tasks"`
- **Use:** `:projects` instead of `:projects="projects"`
- **Use:** `:error` instead of `:error="error"`
- **Use:** `:user` instead of `:user="user"`
- **Use:** `:loading` instead of `:loading="loading"`

IDE inlay hints and Volar automatically provide the full context, keeping template markup concise and idiomatic.
