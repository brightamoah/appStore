# UI Collection Formatting & Helper Conventions

When writing helper functions that filter, process, or limit collections for UI presentation:

1. **Avoid Clumsy Condition-Heavy Identifiers**:
   - Prefer: `getMembersWithAvatars(project, limit = 4)` or `getVisibleItems(list, max = 5)`
   - Avoid: `getMembersWithProfileNotNullOrEmptyMax4(project)`

2. **Always Provide Default Arguments for Count Thresholds**:
   - Provide a sensible default count (e.g., `limit = 4`) so basic calls in templates remain clean (`getMembersWithAvatars(project)`), while allowing custom limits where required (`getMembersWithAvatars(project, 6)`).

3. **Safe Null & Whitespace Guarding**:
   - When filtering string fields (such as image URLs or labels), guard against `null`, `undefined`, and empty/whitespace-only strings:
     ```ts
     Boolean(item.field?.trim())
     ```
