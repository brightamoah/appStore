export function useUserItems() {
  const authStore = useAuthStore();

  const { user } = storeToRefs(authStore);

  const userInitials = computed(() => {
    if (!user.value?.name) return "TF";
    return generateInitials(user.value.name);
  });

  const avatarBgColor = computed(() => {
    if (!user.value || !user.value.id) return "gray";

    return generateUserColor(user.value?.id);
  });

  // const userBadgeColor = computed(() => {
  //   if (!user.value) return "neutral";
  //   return user.value?.role === "admin" ? "success" : "primary";
  // });

  const userFirstName = computed(() => user.value?.name.split(" ").at(0));

  return {
    userInitials,
    avatarBgColor,
    // userBadgeColor,
    userFirstName,
  };
}
