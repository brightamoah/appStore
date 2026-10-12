<script setup lang="ts">
import type { FormErrorEvent, FormSubmitEvent } from "@nuxt/ui";
import type * as z from "zod";

interface Props {
  projectId: string;
  column?: BoardColumn | null;
}

const { projectId, column = null } = defineProps<Props>();

const emit = defineEmits<{
  (event: "saved", savedColumn: BoardColumn): void;
}>();

const open = defineModel<boolean>("open", { default: false });

const boardStore = useBoardStore();

const isEditing = computed<boolean>(() => Boolean(column));

const formRef = useTemplateRef("formRef");
const isSubmitting = ref<boolean>(false);

const state = reactive<{
  name: string;
  statusMapping: TaskStatus;
  wipLimit: number;
  color: string;
}>({
  name: "",
  statusMapping: "NotStarted",
  wipLimit: 0,
  color: "neutral",
});

const metadataStore = useMetadataStore();

const statusOptions = computed(() => metadataStore.statuses);

const colorOptions: { label: string; value: ColorType; class: string }[] = [
  { label: "Neutral", value: "neutral", class: "bg-neutral-500" },
  { label: "Primary", value: "primary", class: "bg-primary" },
  { label: "Success", value: "success", class: "bg-success" },
  { label: "Warning", value: "warning", class: "bg-warning" },
  { label: "Error", value: "error", class: "bg-error" },
  { label: "Info", value: "info", class: "bg-sky-500" },
];

watch(
  () => [open.value, column] as const,
  ([isOpen, currentColumn]) => {
    if (isOpen) {
      if (currentColumn) {
        state.name = currentColumn.name;
        state.statusMapping = currentColumn.statusMapping;
        state.wipLimit = currentColumn.wipLimit;
        state.color = currentColumn.color?.toLowerCase() || "neutral";
      }
      else {
        state.name = "";
        state.statusMapping = "NotStarted";
        state.wipLimit = 0;
        state.color = "neutral";
      }
    }
  },
  { immediate: true },
);

type ColumnFormSchema = z.infer<typeof createColumnSchema>;

async function onSubmit(event: FormSubmitEvent<ColumnFormSchema>) {
  isSubmitting.value = true;
  try {
    let result: BoardColumn;

    if (isEditing.value && column) {
      result = await boardStore.updateColumn(projectId, column.id, {
        name: event.data.name.trim(),
        statusMapping: event.data.statusMapping,
        wipLimit: event.data.wipLimit,
        color: event.data.color,
      });
    }
    else {
      result = await boardStore.createColumn(projectId, {
        name: event.data.name.trim(),
        statusMapping: event.data.statusMapping,
        wipLimit: event.data.wipLimit,
        color: event.data.color,
      });
    }

    emit("saved", result);
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
    :title="isEditing ? 'Edit Column' : 'Add New Column'"
    :description="isEditing
      ? 'Modify column name, WIP limit, status mapping, and color.'
      : 'Create a new Task workflow column for this project.'"
  >
    <template #body>
      <UForm
        ref="formRef"
        class="space-y-4"
        :schema="isEditing ? updateColumnSchema : createColumnSchema"
        :state
        @submit="onSubmit"
        @error="onError"
      >
        <UFormField
          label="Column Name"
          name="name"
          required
          size="sm"
        >
          <UInput
            v-model="state.name"
            placeholder="e.g. In Review, QA, Blocked..."
            autofocus
            class="w-full"
          />
        </UFormField>

        <div class="gap-3 grid grid-cols-2">
          <UFormField
            label="Status"
            name="statusMapping"
            required
            size="sm"
            class="w-full"
          >
            <USelectMenu
              v-model="state.statusMapping"
              :items="statusOptions"
              value-key="value"
              class="w-full"
            />
          </UFormField>

          <UFormField
            label="WIP Limit (0 = Unlimited)"
            name="wipLimit"
            size="sm"
          >
            <UInputNumber
              v-model="state.wipLimit"
              :min="0"
              placeholder="0"
              class="w-full"
            />
          </UFormField>
        </div>

        <UFormField
          label="Column Accent Color"
          name="color"
          size="sm"
        >
          <div class="flex flex-wrap gap-2">
            <UButton
              v-for="colorOption in colorOptions"
              :key="colorOption.value"
              :color="colorOption.value"
              size="xs"
              variant="subtle"
              class="flex items-center rounded-lg text-xs transition-colors"
              :class="[
                state.color === colorOption.value
                  ? `font-medium border`
                  : 'text-muted opacity-75',
              ]"
              @click="state.color = colorOption.value"
            >
              <template #leading>
                <span
                  class="rounded-full size-2.5 shrink-0"
                  :class="colorOption.class"
                />
              </template>

              {{ colorOption.label }}
            </UButton>
          </div>
        </UFormField>

        <div class="flex justify-end gap-2 pt-3 border-default border-t">
          <UButton
            label="Cancel"
            color="error"
            variant="ghost"
            size="sm"
            @click="open = false"
          />

          <UButton
            type="submit"
            :label="isEditing ? 'Save Changes' : 'Create Column'"
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
