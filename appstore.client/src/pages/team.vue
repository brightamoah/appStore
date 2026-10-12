<script setup lang="ts">
import type { AddProjectMemberPayload, ProjectMember, ProjectRole } from "@/types/project";

const projectStore = useProjectStore();
const authStore = useAuthStore();

const isAddMemberModalOpen = ref<boolean>(false);
const searchQuery = ref<string>("");
const memberToRemove = ref<{ userId: string; name: string } | null>(null);
const isConfirmRemoveOpen = ref<boolean>(false);

const newMemberForm = reactive<{
  userId: string;
  role: ProjectRole;
}>({
  userId: "",
  role: "Member",
});

const availableRoleOptions: { label: string; value: ProjectRole }[] = [
  { label: "Admin - Manage tasks and members", value: "Admin" },
  { label: "Member - Create and work on tasks", value: "Member" },
  { label: "Viewer - View tasks and boards", value: "Viewer" },
];

const activeProjectId = computed(() => projectStore.activeProject?.id);

const isSystemAdmin = computed(() => {
  return authStore.hasRole("Admin") || authStore.hasPermission("users.manage");
});

const isProjectOwner = computed(() => {
  return projectStore.activeProject?.ownerId === authStore.user?.id;
});

const currentUserMember = computed(() => {
  return projectStore.currentMembers.find((member: ProjectMember) => member.userId === authStore.user?.id);
});

const canManageMembers = computed(() => {
  return isSystemAdmin.value || isProjectOwner.value || currentUserMember.value?.role === "Admin" || currentUserMember.value?.role === "Owner";
});

const filteredMembers = computed(() => {
  const query = searchQuery.value.trim().toLowerCase();
  if (!query) return projectStore.currentMembers;

  return projectStore.currentMembers.filter((member: ProjectMember) =>
    member.name.toLowerCase().includes(query) || member.email.toLowerCase().includes(query),
  );
});

const availableUserSelectItems = computed(() => {
  return projectStore.availableUsers.map(availableUser => ({
    id: availableUser.id,
    label: `${availableUser.name} (${availableUser.email})`,
    avatar: availableUser.profilePictureUrl ? { src: availableUser.profilePictureUrl } : undefined,
  }));
});

async function loadTeamData() {
  if (projectStore.activeProject?.id) {
    await projectStore.fetchMembers(projectStore.activeProject.id);
    if (canManageMembers.value) {
      await projectStore.fetchAvailableUsers(projectStore.activeProject.id);
    }
  }
}

watch(activeProjectId, async (newId) => {
  if (newId) {
    await loadTeamData();
  }
}, { immediate: true });

function openAddModal() {
  newMemberForm.userId = "";
  newMemberForm.role = "Member";
  if (projectStore.activeProject?.id) {
    projectStore.fetchAvailableUsers(projectStore.activeProject.id);
  }
  isAddMemberModalOpen.value = true;
}

async function handleAddMember() {
  if (!projectStore.activeProject?.id || !newMemberForm.userId) {
    return;
  }

  const payload: AddProjectMemberPayload = {
    userId: newMemberForm.userId,
    role: newMemberForm.role,
  };

  try {
    await projectStore.addMember(projectStore.activeProject.id, payload);
    isAddMemberModalOpen.value = false;
  }
  catch {
    // Handled in store
  }
}

async function handleRoleChange(userId: string, newRole: ProjectRole) {
  if (!projectStore.activeProject?.id) return;
  await projectStore.updateMemberRole(projectStore.activeProject.id, userId, newRole);
}

function promptRemoveMember(userId: string, name: string) {
  memberToRemove.value = { userId, name };
  isConfirmRemoveOpen.value = true;
}

async function confirmRemoveMember() {
  if (!projectStore.activeProject?.id || !memberToRemove.value) return;

  try {
    await projectStore.removeMember(projectStore.activeProject.id, memberToRemove.value.userId);
    isConfirmRemoveOpen.value = false;
    memberToRemove.value = null;
  }
  catch {
    // Handled in store
  }
}

function getRoleBadgeColor(role: ProjectRole): "primary" | "warning" | "neutral" | "info" {
  switch (role) {
    case "Owner": return "primary";
    case "Admin": return "warning";
    case "Member": return "info";
    case "Viewer":
    default:
      return "neutral";
  }
}

function getMemberActionItems(member: { userId: string; role: ProjectRole; name: string }) {
  const isOwner = member.role === "Owner";
  if (isOwner || !canManageMembers.value) return [];

  return [
    [
      {
        label: "Make Admin",
        icon: "i-lucide:shield",
        disabled: member.role === "Admin",
        onSelect: () => handleRoleChange(member.userId, "Admin"),
      },
      {
        label: "Make Member",
        icon: "i-lucide:user",
        disabled: member.role === "Member",
        onSelect: () => handleRoleChange(member.userId, "Member"),
      },
      {
        label: "Make Viewer",
        icon: "i-lucide:eye",
        disabled: member.role === "Viewer",
        onSelect: () => handleRoleChange(member.userId, "Viewer"),
      },
    ],
    [
      {
        label: "Remove from Project",
        icon: "i-lucide:user-minus",
        color: "error" as const,
        onSelect: () => promptRemoveMember(member.userId, member.name),
      },
    ],
  ];
}
</script>

<template>
  <Dashboard
    variant="floating"
    collapsible="icon"
  >
    <template #header-title>
      <div class="flex items-center gap-2">
        <h2 class="font-medium text-highlighted text-sm">
          Team Management
        </h2>

        <UBadge
          v-if="projectStore.activeProject"
          :label="projectStore.activeProject.name"
          variant="subtle"
          color="neutral"
          size="xs"
        />
      </div>
    </template>

    <div class="space-y-6 max-w-5xl">
      <!-- HEADER SECTION -->
      <div class="flex sm:flex-row flex-col justify-between items-start sm:items-center gap-4">
        <div>
          <h1 class="font-mono font-bold text-highlighted text-2xl tracking-tight">
            Project Members
          </h1>

          <p class="mt-1 text-muted text-sm">
            Manage teammates with access to
            <span class="font-semibold text-highlighted">{{ projectStore.activeProject?.name || "your project" }}</span>.
            Only project members can be assigned to tasks.
          </p>
        </div>

        <div class="flex items-center gap-2">
          <UButton
            v-if="canManageMembers"
            icon="i-lucide:user-plus"
            label="Add Member"
            color="primary"
            size="sm"
            @click="openAddModal"
          />
        </div>
      </div>

      <!-- FILTER & STATS BAR -->
      <div class="flex sm:flex-row flex-col justify-between items-stretch sm:items-center gap-3">
        <div class="w-full sm:w-72">
          <UInput
            v-model="searchQuery"
            icon="i-lucide:search"
            placeholder="Search members..."
            size="sm"
            class="w-full"
          />
        </div>

        <div class="flex items-center gap-2 text-muted text-xs">
          <span>{{ projectStore.currentMembers.length }} Total Member{{ projectStore.currentMembers.length === 1 ? '' : 's' }}</span>
        </div>
      </div>

      <!-- MEMBERS LIST -->
      <div
        v-if="projectStore.isMembersLoading"
        class="py-12 text-center text-muted text-sm"
      >
        <UIcon
          name="i-lucide:loader"
          class="size-6 animate-spin mx-auto mb-2 text-primary"
        />
        Loading project members...
      </div>

      <div
        v-else-if="filteredMembers.length === 0"
        class="flex flex-col items-center justify-center p-12 border border-dashed border-default rounded-2xl bg-muted/20 text-center"
      >
        <UIcon
          name="i-lucide:users"
          class="size-10 text-muted mb-3"
        />

        <h3 class="font-semibold text-highlighted text-sm">
          No members found
        </h3>

        <p class="text-muted text-xs mt-1 max-w-sm">
          {{ searchQuery ? "No teammates match your search criteria." : "There are currently no members assigned to this project." }}
        </p>

        <UButton
          v-if="canManageMembers && !searchQuery"
          icon="i-lucide:user-plus"
          label="Add First Member"
          color="primary"
          size="xs"
          class="mt-4"
          @click="openAddModal"
        />
      </div>

      <div
        v-else
        class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4"
      >
        <div
          v-for="member in filteredMembers"
          :key="member.userId"
          class="flex flex-col justify-between bg-elevated/40 p-4 border border-default rounded-xl hover:border-default/80 transition-all space-y-3"
        >
          <div class="flex items-start justify-between gap-3">
            <div class="flex items-center gap-3 min-w-0">
              <UAvatar
                v-if="member.profilePictureUrl"
                :src="member.profilePictureUrl"
                :alt="member.name"
                size="md"
              />

              <div
                v-else
                class="flex justify-center items-center bg-primary/20 rounded-full size-10 font-semibold text-primary text-sm shrink-0"
              >
                {{ member.name.slice(0, 2).toUpperCase() }}
              </div>

              <div class="min-w-0">
                <div class="flex items-center gap-1.5">
                  <h3 class="font-semibold text-highlighted text-sm truncate">
                    {{ member.name }}
                  </h3>

                  <span
                    v-if="member.userId === authStore.user?.id"
                    class="text-xs text-muted font-normal"
                  >(You)</span>
                </div>

                <p class="text-muted text-xs truncate">
                  {{ member.email }}
                </p>
              </div>
            </div>

            <UDropdownMenu
              v-if="canManageMembers && member.role !== 'Owner'"
              :items="getMemberActionItems(member)"
            >
              <UButton
                icon="i-lucide:more-vertical"
                color="neutral"
                variant="ghost"
                size="xs"
                aria-label="Manage member"
              />
            </UDropdownMenu>
          </div>

          <div class="flex items-center justify-between pt-2 border-t border-default/50 text-xs">
            <UBadge
              :label="member.role"
              :color="getRoleBadgeColor(member.role)"
              variant="subtle"
              size="xs"
              class="font-semibold"
            />

            <span class="text-muted text-xs">
              Joined {{ new Date(member.joinedAt).toLocaleDateString() }}
            </span>
          </div>
        </div>
      </div>
    </div>

    <!-- ADD MEMBER MODAL -->
    <UModal
      v-model:open="isAddMemberModalOpen"
      title="Add Member to Project"
      description="Select an existing workspace user to invite to this project."
    >
      <template #body>
        <div class="space-y-4">
          <UFormField
            label="Workspace User"
            required
            size="sm"
          >
            <USelectMenu
              v-model="newMemberForm.userId"
              :items="availableUserSelectItems"
              value-key="id"
              label-key="label"
              placeholder="Search and select a user..."
              icon="i-lucide:user"
              class="w-full"
            />

            <template
              v-if="availableUserSelectItems.length === 0"
              #help
            >
              <span class="text-muted text-xs">All workspace users are already in this project.</span>
            </template>
          </UFormField>

          <UFormField
            label="Project Role"
            required
            size="sm"
          >
            <USelectMenu
              v-model="newMemberForm.role"
              :items="availableRoleOptions"
              value-key="value"
              label-key="label"
              class="w-full"
            />
          </UFormField>

          <div class="flex justify-end gap-2 pt-3 border-t border-default">
            <UButton
              label="Cancel"
              color="neutral"
              variant="ghost"
              size="sm"
              @click="isAddMemberModalOpen = false"
            />

            <UButton
              label="Add Member"
              color="primary"
              size="sm"
              :disabled="!newMemberForm.userId"
              :loading="projectStore.isSaving"
              @click="handleAddMember"
            />
          </div>
        </div>
      </template>
    </UModal>

    <!-- CONFIRM REMOVE MODAL -->
    <UModal
      v-model:open="isConfirmRemoveOpen"
      title="Remove Member"
      :description="`Are you sure you want to remove ${memberToRemove?.name} from this project? They will no longer be eligible for task assignments in this project.`"
    >
      <template #body>
        <div class="flex justify-end gap-2 pt-2">
          <UButton
            label="Cancel"
            color="neutral"
            variant="ghost"
            size="sm"
            @click="isConfirmRemoveOpen = false"
          />

          <UButton
            label="Remove Member"
            color="error"
            size="sm"
            :loading="projectStore.isSaving"
            @click="confirmRemoveMember"
          />
        </div>
      </template>
    </UModal>
  </Dashboard>
</template>

<style scoped>
</style>
