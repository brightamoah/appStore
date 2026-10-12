using appStore.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace appStore.Server.Tests
{
    public class EntityModelCycleTests
    {
        [Fact]
        public void EntityModel_ShouldHaveNoCircularNavigationLoops()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;

            using var context = new DatabaseContext(options);
            var model = context.Model;

            var cycles = new List<string>();

            foreach (var entity in model.GetEntityTypes())
            {
                var visited = new HashSet<IEntityType>();
                var recursionStack = new List<IEntityType>();

                FindCycles(entity, null, visited, recursionStack, cycles);
            }

            Assert.True(
                cycles.Count == 0, 
                $"Found circular loops in entity navigation:\n{string.Join("\n", cycles.Distinct())}"
            );
        }

        private static void FindCycles(
            IEntityType current, 
            INavigation? incomingNav,
            HashSet<IEntityType> visited, 
            List<IEntityType> stack, 
            List<string> cycles)
        {
            var stackIndex = stack.IndexOf(current);
            if (stackIndex >= 0)
            {
                var cyclePath = string.Join(" -> ", stack.Skip(stackIndex).Select(e => e.ClrType.Name)) 
                    + " -> " + current.ClrType.Name;
                cycles.Add(cyclePath);
                return;
            }

            stack.Add(current);

            foreach (var nav in current.GetNavigations())
            {
                // Skip trivial backtracking across the exact same foreign key inverse
                // (e.g. BoardColumn -> Project and immediately back along Project.Columns)
                if (incomingNav != null && nav == incomingNav.Inverse)
                {
                    continue;
                }

                // Skip self-referencing hierarchy (ParentTask -> Subtasks on same entity)
                if (nav.TargetEntityType == current)
                {
                    continue;
                }

                FindCycles(nav.TargetEntityType, nav, visited, stack, cycles);
            }

            stack.RemoveAt(stack.Count - 1);
        }
    }
}