export const useWeatherStore = defineStore("weatherStore", () => {
  const responseData = ref<WeatherForecast[] | null>(null);
  const isLoading = ref(false);

  const getWeather = async () => {
    isLoading.value = true;
    try {
      const response = await api.get("/api/weatherForecast");

      responseData.value = response;
      // console.log(response)
    }
    finally {
      isLoading.value = false;
    }
  };

  return {
    getWeather,
    responseData,
    isLoading,
  };
});

if (import.meta.hot) {
  import.meta.hot.accept(acceptHMRUpdate(useWeatherStore, import.meta.hot));
}
