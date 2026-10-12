<script setup lang="ts">
import type { FormErrorEvent, FormSubmitEvent } from "@nuxt/ui";
import type * as z from "zod";

const emit = defineEmits<{
  (event: "created", createdProject: ProjectWithMembers): void;
}>();

const open = defineModel<boolean>("open", { default: false });

const projectStore = useProjectStore();

const isSubmitting = ref<boolean>(false);

const state = reactive<{
  name: string;
  key: string;
  description: string;
  icon: string;
}>({
  name: "",
  key: "",
  description: "",
  icon: "frontend",
});

function handleNameChange(newName: string) {
  state.name = newName;
  if (!state.key || state.key.length < 3) {
    const cleanWord = newName.replace(/[^a-z]/gi, "").toUpperCase();
    if (cleanWord.length >= 2) {
      state.key = cleanWord.slice(0, 4);
    }
  }
}

watch(open, (isOpen) => {
  if (isOpen) {
    state.name = "";
    state.key = "";
    state.description = "";
    state.icon = "frontend";
  }
});

type ProjectFormValues = z.infer<typeof createProjectSchema>;

async function onSubmit(event: FormSubmitEvent<ProjectFormValues>) {
  isSubmitting.value = true;
  try {
    const createdProject = await projectStore.createProject({
      name: event.data.name.trim(),
      key: event.data.key.trim().toUpperCase(),
      description: event.data.description?.trim() || "",
      icon: event.data.icon || "frontend",
    });

    emit("created", createdProject);
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
    title="Create New Project"
    description="Provision a new project workspace with dedicated Kanban columns and Jira-style issue keys."
  >
    <template #body>
      <UForm
        :schema="createProjectSchema"
        :state
        class="space-y-4"
        @submit="onSubmit"
        @error="onError"
      >
        <UFormField
          label="Project Name"
          name="name"
          required
          size="sm"
        >
          <UInput
            :model-value="state.name"
            placeholder="e.g. Core Application"
            autofocus
            class="w-full"
            @update:model-value="handleNameChange"
          />
        </UFormField>

        <UFormField
          label="Project Key"
          name="key"
          required
          help="Used as prefix for tasks, e.g. CORE-1"
          size="sm"
        >
          <UInput
            v-model="state.key"
            placeholder="e.g. CORE"
            maxlength="10"
            class="w-full font-mono uppercase"
          />
        </UFormField>

        <UFormField
          label="Description"
          name="description"
          size="sm"
        >
          <UTextarea
            v-model="state.description"
            placeholder="Short description of the project goals..."
            :rows="3"
            class="w-full"
          />
        </UFormField>

        <div class="flex justify-end gap-2 pt-3 border-default border-t">
          <UButton
            label="Cancel"
            color="neutral"
            variant="ghost"
            size="sm"
            @click="open = false"
          />

          <UButton
            label="Create Project"
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
