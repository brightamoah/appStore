import child_process from "node:child_process";

import fs from "node:fs";
import path from "node:path";
import { env } from "node:process";
import { fileURLToPath, URL } from "node:url";
import ui from "@nuxt/ui/vite";
import plugin from "@vitejs/plugin-vue";
import { defineConfig } from "vite";
import vueDevTools from "vite-plugin-vue-devtools";
import VueRouter from "vue-router/vite";

const baseFolder
  = env.APPDATA !== undefined && env.APPDATA !== ""
    ? `${env.APPDATA}/ASP.NET/https`
    : `${env.HOME}/.aspnet/https`;

const certificateName = "appstore.client";
const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

if (!fs.existsSync(baseFolder)) {
  fs.mkdirSync(baseFolder, { recursive: true });
}

if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
  if (child_process.spawnSync("dotnet", [
    "dev-certs",
    "https",
    "--export-path",
    certFilePath,
    "--format",
    "Pem",
    "--no-password",
  ], { stdio: "inherit" }).status !== 0) {
    throw new Error("Could not create certificate.");
  }
}

const target = env.ASPNETCORE_HTTPS_PORT
  ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}`
  : env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(";")[0] : "https://localhost:7089";

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    VueRouter({}),
    plugin(),
    ui({
      ui: {
        colors: {
          neutral: "neutral",
          primary: "oxford-blue",
          secondary: "tan",
        },
        icons: {
          light: "i-lucide:moon-star",
          dark: "i-lucide:sun",
        },
        navigationMenu: {
          compoundVariants: [
            {
              variant: "pill",
              active: true,
              highlight: false,
              class: {
                link: "before:bg-primary/10",
              },
            },
          ],
        },
      },
      autoImport: {
        imports: [
          "vue",
          "pinia",
          "vue-router",
          "@vueuse/core",
        ],
        dirs: [
          "./src/stores",
          "./src/composables",
          "./src/services",
          "./src/utils",
          {
            glob: "./src/types",
            types: true,
          },
        ],
        vueTemplate: true,
        dts: true,
        eslintrc: {
          enabled: true,
          filepath: "./.eslintrc-auto-import.json",
          globalsPropValue: true,
        },
      },
      components: {
        dirs: [
          "./src/components",
          "./src/layouts",
        ],
        extensions: [
          "vue",
        ],
        dts: true,
        directoryAsNamespace: true,
        collapseSamePrefixes: true,
        globalNamespaces: ["UI", "ui"],
      },
      icon: {
        clientBundle: {
          scan: true,
        },
      },
    }),
    vueDevTools(),
  ],

  resolve: {
    alias: {
      "@": fileURLToPath(new URL("./src", import.meta.url)),
    },
  },
  server: {
    proxy: {
      "/weatherforecast": {
        target,
        changeOrigin: true,
        secure: false,
      },
      "/api": {
        target,
        changeOrigin: true,
        secure: false,
      },
    },
    port: parseInt(env.DEV_SERVER_PORT || "63175"),
    https: {
      key: fs.readFileSync(keyFilePath),
      cert: fs.readFileSync(certFilePath),
    },
  },
});
