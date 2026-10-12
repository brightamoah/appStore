import type { ProjectIcon } from "@/utils/iconMap";

export interface Project {
  id: string;
  name: string;
  key: string;
  description?: string;
  icon?: ProjectIcon | (string & {});
  ownerId: string;
  createdAt: string;
  updatedAt: string;
}

export type ProjectRole = "Owner" | "Admin" | "Member" | "Viewer";

export interface ProjectMember {
  userId: string;
  name: string;
  email: string;
  profilePictureUrl?: string | null;
  role: ProjectRole;
  joinedAt: string;
}

export type ProjectWithMembers = Pick<Project, "id" | "name" | "key" | "ownerId" | "description" | "icon"> & {
  members: (Pick<User, "id" | "email" | "name" | "profilePictureUrl"> & { role?: ProjectRole })[];
};

export interface CreateProjectPayload {
  name: string;
  key: string;
  description?: string;
  icon?: string;
}

export interface AddProjectMemberPayload {
  userId: string;
  role: ProjectRole;
}

export interface UpdateProjectMemberRolePayload {
  role: ProjectRole;
}
