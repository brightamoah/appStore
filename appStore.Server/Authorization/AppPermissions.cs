namespace appStore.Server.Authorization
{
	public static class AppPermissions
	{
		public static class Tasks
		{
			public const string Read = "tasks.read";
			public const string Create = "task.create";
			public const string Update = "task.update";
			public const string Delete = "task.delete";
		}

		public static class Users
		{
			public const string Read = "users.read";
			public const string Create = "users.create";
			public const string Update = "users.update";
			public const string Delete = "users.delete";
			public const string Manage = "users.manage";
		}

		public static class Roles
		{
			public const string Read = "roles.read";
			public const string Manage = "roles.manage";
		}

		public static class Permissions
		{
			public const string Read = "permissions.read";
			public const string Manage = "permissions.manage";
		}

		public static class Reports
		{
			public const string Read = "reports.read";
			public const string Export = "reports.export";
		}

		public static class Security
		{
			public const string Manage = "security.manage";
		}

		public static class Priority
		{
			public const string Read = "priority.read";
			public const string Create = "priority.create";
			public const string Update = "priority.update";
			public const string Delete = "priority.delete";
		}

		public sealed record PermissionMetadata(string Name, string Description, string Category);

		public static IReadOnlyList<PermissionMetadata> All { get; } =
		[
			new(
				Tasks.Read,
				"Can read all tasks",
				nameof( Tasks )
			 ),

			new(
				Tasks.Create,
				"Can create new tasks",
				nameof( Tasks )
			 ),

			new(
				Tasks.Update,
				"Can update existing tasks",
				nameof( Tasks )
			 ),

			new(
				Tasks.Delete,
				"Can delete tasks",
				nameof( Tasks )
			),

			new(
				Users.Read,
				"View users and user profiles",
				nameof( Users )
			),

			new(
				Users.Create,
				"Create new users",
				nameof( Users )
			),

			new(
				Users.Update,
				"Update existing users",
				nameof( Users )
			),


			new(
				 Users.Delete,
				 "Deactivate or delete users",
				 nameof( Users )
			),


			new(
				 Users.Manage,
				 "Create, update, deactivate, and manage users",
				 nameof( Users )
			),



			new(
				Roles.Read,
				"View system roles and assigned permissions",
				nameof( Roles )
			),

			new(
				Roles.Manage,
				"Create, update, and delete roles and role permissions",
				nameof( Roles )
			),



			new(
				Permissions.Read,
				"View available system permissions",
				nameof( Permissions )
			),

			new(
				Permissions.Manage,
				"Manage system permissions",
				nameof( Permissions )
			),


			new(
				Reports.Read,
				"View analytics and reports",
				nameof( Reports )
			),

			new(
				Reports.Export,
				"Export reports and system data",
				nameof( Reports )
			),


			new(
				Security.Manage,
				"Manage security settings and password policies",
				nameof( Security )
			),

			new(
				Priority.Create,
				"Create new priority",
				nameof( Priority )
			),

			new(
				Priority.Read,
				"View existing priorities",
				nameof( Priority )
			),

			new(
				Priority.Update,
				"Update existing priorities",
				nameof( Priority )
			),

			new(
				Priority.Delete,
				"Delete existing priorities",
				nameof( Priority )
			)
		];
	}
}
