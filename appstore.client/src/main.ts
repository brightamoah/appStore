import { autoAnimatePlugin } from "@formkit/auto-animate/vue";

import ui from "@nuxt/ui/vue-plugin";
import { createPinia } from "pinia";
import piniaPluginPersistedstate from "pinia-plugin-persistedstate";
import { createApp } from "vue";
import App from "./App.vue";
import { router } from "./router/index";
import "./assets/main.css";

const pinia = createPinia();
pinia.use(piniaPluginPersistedstate);

const app = createApp(App);

app.use(pinia);
app.use(router);
app.use(ui);
app.use(autoAnimatePlugin);
app.mount("#app");
