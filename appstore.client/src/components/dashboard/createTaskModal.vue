<script setup lang="ts">
import type { FormErrorEvent, FormSubmitEvent } from "@nuxt/ui";
import * as z from "zod";

interface Props {
  projectId: string;
  defaultColumnId?: string;
}

const { projectId, defaultColumnId } = defineProps<Props>();

const emit = defineEmits<{
  (event: "created"): void;
}>();

const open = defineModel<boolean>("open", { default: false });

const taskStore = useTaskStore();
const projectStore = useProjectStore();
const authStore = useAuthStore();
const boardStore = useBoardStore();
const metadataStore = useMetadataStore();

const isSubmitting = ref<boolean>(false);

const state = reactive<{
  name: string;
  description: string;
  priorityId: number;
  issueType: IssueType;
  assigneeIds: string[];
  dueDate: string;
  estimatedMinutes?: number;
}>({
  name: "",
  description: "",
  priorityId: 2,
  issueType: "Task",
  assigneeIds: [],
  dueDate: "",
  estimatedMinutes: undefined,
});

const issueTypes = computed(() => metadataStore.issueTypes.map(item => item.value));
const priorityOptions = computed(() => metadataStore.priorities);

const projectMembers = computed(() => projectStore.activeProject?.members || []);

const targetColumn = computed(() => {
  if (!defaultColumnId) return null;
  return boardStore.columns.find(column => column.id === defaultColumnId) || null;
});

const isTargetColumnInProgress = computed(() => {
  return targetColumn.value?.statusMapping === "InProgress";
});

const selectedAssignees = computed(() => {
  return projectMembers.value.filter(member => state.assigneeIds.includes(member.id));
});

function removeAssignee(userId: string) {
  state.assigneeIds = state.assigneeIds.filter(id => id !== userId);
}

function assignToMe() {
  if (authStore.user?.id && !state.assigneeIds.includes(authStore.user.id)) {
    state.assigneeIds.push(authStore.user.id);
  }
}

const assigneeSelectItems = computed(() => {
  return projectMembers.value.map(member => ({
    id: member.id,
    label: member.id === authStore.user?.id ? `${member.name} (You)` : member.name,
    avatar: member.profilePictureUrl ? { src: member.profilePictureUrl } : undefined,
  }));
});

// Auto-suggest self-assignment when creating directly in InProgress column
watch(open, (isOpen) => {
  if (isOpen) {
    state.name = "";
    state.description = "";
    state.priorityId = 2;
    state.issueType = "Task";
    state.dueDate = "";
    state.estimatedMinutes = undefined;

    if (isTargetColumnInProgress.value && authStore.user?.id && !state.assigneeIds.includes(authStore.user.id)) {
      state.assigneeIds = [authStore.user.id];
    }
    else {
      state.assigneeIds = [];
    }
  }
});

const formSchema = computed(() => {
  return createTaskSchema.superRefine((data, ctx) => {
    if (isTargetColumnInProgress.value) {
      const hasAssignee = (data.assigneeIds && data.assigneeIds.length > 0) || Boolean(data.assigneeId);
      if (!hasAssignee) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          message: "Tasks created directly in 'In Progress' require an assigned user",
          path: ["assigneeIds"],
        });
      }
    }
  });
});

type CreateTaskFormValues = z.infer<typeof createTaskSchema>;

async function onSubmit(event: FormSubmitEvent<CreateTaskFormValues>) {
  isSubmitting.value = true;
  try {
    await taskStore.createTask({
      name: event.data.name.trim(),
      description: event.data.description?.trim() || "",
      priorityId: event.data.priorityId,
      projectId,
      columnId: defaultColumnId,
      issueType: event.data.issueType,
      assigneeId: event.data.assigneeIds?.[0] || undefined,
      assigneeIds: event.data.assigneeIds || [],
      dueDate: event.data.dueDate ? new Date(event.data.dueDate).toISOString() : undefined,
      estimatedMinutes: event.data.estimatedMinutes ? Number(event.data.estimatedMinutes) : undefined,
    });

    emit("created");
    open.value = false;
  }
  finally {
    isSubmitting.value = false;
  }
}

function onError(event: FormErrorEvent) {
  if (event?.errors?.[0]?.id) {
    const element = document.getElementById(event.errors[0].id);
    element?.focus();
    element?.scrollIntoView({ behavior: "smooth", block: "center" });
  }
}
</script>

<template>
  <UModal
    v-model:open="open"
    title="Create New Task"
    description="Add a new task to your board with Jira-style sequential numbering."
  >
    <template #body>
      <UForm
        :schema="formSchema"
        :state
        class="space-y-4"
        @submit="onSubmit"
        @error="onError"
      >
        <UFormField
          label="Summary / Title"
          name="name"
          size="sm"
          required
        >
          <UInput
            v-model="state.name"
            placeholder="e.g. Implement authentication middleware"
            autofocus
            class="w-full"
          />
        </UFormField>

        <div class="gap-3 grid grid-cols-2">
          <UFormField
            label="Issue Type"
            name="issueType"
            size="sm"
            required
          >
            <div class="flex gap-1">
              <UButton
                v-for="typeOption in issueTypes"
                :key="typeOption"
                :label="typeOption"
                :variant="state.issueType === typeOption ? 'solid' : 'subtle'"
                :color="state.issueType === typeOption ? 'primary' : 'neutral'"
                size="xs"
                @click="state.issueType = typeOption"
              />
            </div>
          </UFormField>

          <UFormField
            label="Priority"
            name="priorityId"
            size="sm"
            required
          >
            <div class="flex gap-1">
              <UButton
                v-for="priorityOption in priorityOptions"
                :key="priorityOption.id"
                :label="priorityOption.name"
                :variant="state.priorityId === priorityOption.id ? 'solid' : 'subtle'"
                :color="state.priorityId === priorityOption.id ? priorityOption.color : 'neutral'"
                size="xs"
                @click="state.priorityId = priorityOption.id"
              />
            </div>
          </UFormField>
        </div>

        <UFormField
          label="Assignees"
          name="assigneeIds"
          size="sm"
        >
          <template #hint>
            <UButton
              v-if="authStore.user && !state.assigneeIds.includes(authStore.user.id)"
              label="Assign to me"
              color="primary"
              variant="link"
              size="xs"
              class="p-0 h-auto font-normal text-xs"
              @click="assignToMe"
            />
          </template>

          <USelectMenu
            v-model="state.assigneeIds"
            :items="assigneeSelectItems"
            value-key="id"
            label-key="label"
            multiple
            clear
            icon="i-lucide:users"
            placeholder="Select assignees..."
            class="w-full"
          />

          <div
            v-if="selectedAssignees.length > 0"
            class="flex flex-wrap gap-1.5 mt-2"
          >
            <UBadge
              v-for="member in selectedAssignees"
              :key="member.id"
              :label="member.name"
              variant="subtle"
              color="neutral"
              size="xs"
              class="gap-1"
            >
              <button
                type="button"
                class="inline-flex items-center hover:text-error"
                @click="removeAssignee(member.id)"
              >
                <UIcon
                  name="i-lucide:x"
                  class="size-3"
                />
              </button>
            </UBadge>
          </div>

          <p
            v-if="isTargetColumnInProgress && state.assigneeIds.length === 0"
            class="flex items-center gap-1 mt-1 font-medium text-amber-500 text-xs"
          >
            <UIcon
              name="i-lucide:alert-circle"
              class="w-3.5 h-3.5"
            />
            Tasks created directly in 'In Progress' require an assigned user.
          </p>
        </UFormField>

        <UFormField
          label="Description"
          name="description"
          size="sm"
        >
          <UTextarea
            v-model="state.description"
            placeholder="Provide context, acceptance criteria, or checklist details..."
            :rows="3"
            class="w-full"
          />
        </UFormField>

        <div class="gap-3 grid grid-cols-2">
          <UFormField
            label="Due Date"
            name="dueDate"
            size="sm"
          >
            <DatePicker
              v-model="state.dueDate"
              placeholder="Pick a due date"
            />
          </UFormField>

          <UFormField
            label="Estimated Time (Minutes)"
            name="estimatedMinutes"
            size="sm"
          >
            <UInputNumber
              v-model="state.estimatedMinutes"
              :min="0"
              placeholder="e.g. 60"
              class="w-full"
            />
          </UFormField>
        </div>

        <div class="flex justify-end gap-2 pt-3 border-default border-t">
          <UButton
            label="Cancel"
            color="error"
            variant="ghost"
            size="sm"
            @click="open = false"
          />

          <UButton
            label="Create Task"
            type="submit"
            color="primary"
            size="sm"
            :loading="isSubmitting"
          />
        </div>
      </UForm>
    </template>
  </UModal>
</template>

<style scoped>

</style>
