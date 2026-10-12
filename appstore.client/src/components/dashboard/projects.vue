<script setup lang="ts">
const { projects } = defineProps<{
  projects: ProjectWithMembers[];
}>();

/**
 * Returns project members who have a valid avatar URL, capped at a maximum count.
 *
 * @param project - The project containing member records
 * @param limit - Maximum number of avatars to return (defaults to 4)
 */
function getMembersWithAvatars(project: ProjectWithMembers, limit = 4) {
  return project.members
    .filter(member => Boolean(member.profilePictureUrl?.trim()))
    .slice(0, limit);
}
</script>

<template>
  <UCard
    class="shadow-md border border-default rounded-2xl"
    :ui="{
      header: 'p-2 sm:px-4',
      body: 'p-2 sm:p-2.5',
    }"
  >
    <template #header>
      <div class="flex justify-between items-center">
        <h3 class="font-manrope font-bold text-sm">
          Projects
        </h3>

        <UButton
          variant="link"
          color="neutral"
          size="sm"
          icon="i-lucide:ellipsis-vertical"
        />
      </div>
    </template>

    <template #default>
      <div
        v-for="project in projects"
        :key="project.id"
        class="flex justify-between items-center gap-3 py-2 first:pt-0 last:pb-0 border-default border-b last:border-b-0"
      >
        <div class="flex flex-1 items-center gap-2.5 min-w-0 cursor-pointer">
          <UIcon
            :name="getProjectIcon(project.icon)"
            class="size-5 text-primary shrink-0"
          />

          <UTooltip
            :text="project.name"
            class="flex-1 min-w-0"
          >
            <span class="block font-manrope font-medium text-highlighted text-xs truncate">
              {{ project.name }}
            </span>
          </UTooltip>
        </div>

        <UAvatarGroup
          size="xs"
          class="shrink-0"
        >
          <UAvatar
            v-for="member in getMembersWithAvatars(project)"
            :key="member.id"
            :src="member.profilePictureUrl || ''"
            :alt="member.name"
          />
        </UAvatarGroup>
      </div>
    </template>
  </UCard>
</template>

<style scoped>

</style>
