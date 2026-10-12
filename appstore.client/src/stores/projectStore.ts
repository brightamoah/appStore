import type { AddProjectMemberPayload, ProjectMember, ProjectRole } from "@/types/project";

export const useProjectStore = defineStore("projectStore", () => {
  const toast = useToast();

  const projects = ref<ProjectWithMembers[]>([]);
  const activeProjectId = ref<string | null>(null);
  const currentMembers = ref<ProjectMember[]>([]);
  const availableUsers = ref<User[]>([]);
  const isMembersLoading = ref<boolean>(false);
  const isLoading = ref<boolean>(false);
  const isSaving = ref<boolean>(false);
  const error = ref<AppError | null>(null);

  const activeProject = computed<ProjectWithMembers | null>(() => {
    if (!activeProjectId.value) {
      return projects.value[0] || null;
    }
    return projects.value.find(project => project.id === activeProjectId.value) || projects.value[0] || null;
  });

  const clearError = () => {
    error.value = null;
  };

  const fetchProjects = async () => {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.get<{ message: string; data: ProjectWithMembers[] }>("/api/v1/projects");
      projects.value = response.data;

      if (!activeProjectId.value || !projects.value.some(project => project.id === activeProjectId.value)) {
        const firstProject = projects.value[0];
        if (firstProject) {
          activeProjectId.value = firstProject.id;
        }
      }

      return response.data;
    }
    catch (caughtError) {
      error.value = resolveError(caughtError);
      throw caughtError;
    }
    finally {
      isLoading.value = false;
    }
  };

  const createProject = async (payload: CreateProjectPayload) => {
    isSaving.value = true;
    try {
      const response = await api.post<{ message: string; data: ProjectWithMembers }>("/api/v1/projects", payload);
      projects.value.unshift(response.data);
      activeProjectId.value = response.data.id;

      toast.add({
        title: "Project created",
        description: `Project "${response.data.name}" created successfully.`,
        color: "success",
      });

      return response.data;
    }
    catch (caughtError) {
      const resolvedError = resolveError(caughtError);
      toast.add({
        title: "Failed to create project",
        description: resolvedError.message,
        color: "error",
      });
      throw caughtError;
    }
    finally {
      isSaving.value = false;
    }
  };

  const fetchMembers = async (projectId: string) => {
    isMembersLoading.value = true;
    try {
      const response = await api.get<{ message: string; data: ProjectMember[] }>(`/api/v1/projects/${projectId}/members`);
      currentMembers.value = response.data;

      const active = projects.value.find(p => p.id === projectId);
      if (active) {
        active.members = response.data.map(member => ({
          id: member.userId,
          name: member.name,
          email: member.email,
          profilePictureUrl: member.profilePictureUrl ?? null,
          role: member.role,
        }));
      }

      return response.data;
    }
    catch (caughtError) {
      error.value = resolveError(caughtError);
      throw caughtError;
    }
    finally {
      isMembersLoading.value = false;
    }
  };

  const fetchAvailableUsers = async (projectId: string) => {
    try {
      const response = await api.get<{ message: string; data: User[] }>(`/api/v1/projects/${projectId}/available-users`);
      availableUsers.value = response.data;
      return response.data;
    }
    catch (caughtError) {
      error.value = resolveError(caughtError);
      throw caughtError;
    }
  };

  const addMember = async (projectId: string, payload: AddProjectMemberPayload) => {
    isSaving.value = true;
    try {
      const response = await api.post<{ message: string; data: ProjectMember }>(`/api/v1/projects/${projectId}/members`, payload);
      await fetchMembers(projectId);
      toast.add({
        title: "Member added",
        description: `${response.data.name} added to the project.`,
        color: "success",
      });
      return response.data;
    }
    catch (caughtError) {
      const resolved = resolveError(caughtError);
      toast.add({
        title: "Failed to add member",
        description: resolved.message,
        color: "error",
      });
      throw caughtError;
    }
    finally {
      isSaving.value = false;
    }
  };

  const updateMemberRole = async (projectId: string, userId: string, role: ProjectRole) => {
    isSaving.value = true;
    try {
      const response = await api.patch<{ message: string; data: ProjectMember }>(`/api/v1/projects/${projectId}/members/${userId}`, { role });
      await fetchMembers(projectId);
      toast.add({
        title: "Role updated",
        description: `Member role updated to ${role}.`,
        color: "success",
      });
      return response.data;
    }
    catch (caughtError) {
      const resolved = resolveError(caughtError);
      toast.add({
        title: "Failed to update role",
        description: resolved.message,
        color: "error",
      });
      throw caughtError;
    }
    finally {
      isSaving.value = false;
    }
  };

  const removeMember = async (projectId: string, userId: string) => {
    isSaving.value = true;
    try {
      await api.delete(`/api/v1/projects/${projectId}/members/${userId}`);
      await fetchMembers(projectId);
      toast.add({
        title: "Member removed",
        description: "Member removed from the project.",
        color: "neutral",
      });
    }
    catch (caughtError) {
      const resolved = resolveError(caughtError);
      toast.add({
        title: "Failed to remove member",
        description: resolved.message,
        color: "error",
      });
      throw caughtError;
    }
    finally {
      isSaving.value = false;
    }
  };

  const resetState = () => {
    projects.value = [];
    activeProjectId.value = null;
    currentMembers.value = [];
    availableUsers.value = [];
    isMembersLoading.value = false;
    isLoading.value = false;
    isSaving.value = false;
    error.value = null;
  };

  return {
    projects,
    activeProjectId,
    activeProject,
    currentMembers,
    availableUsers,
    isMembersLoading,
    isLoading,
    isSaving,
    error,
    clearError,
    fetchProjects,
    createProject,
    fetchMembers,
    fetchAvailableUsers,
    addMember,
    updateMemberRole,
    removeMember,
    resetState,
  };
}, {
  persist: {
    key: "app_store_project",
    storage: localStorage,
    pick: ["activeProjectId"],
  },
});

if (import.meta.hot) {
  import.meta.hot.accept(acceptHMRUpdate(useProjectStore, import.meta.hot));
}
