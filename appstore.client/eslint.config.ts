import fs from "node:fs";
import antfu from "@antfu/eslint-config";
import { plugin as shadcn } from "@shadcn/lint";
import tsParser from "@typescript-eslint/parser";
import pluginOxlint from "eslint-plugin-oxlint";
import { globalIgnores } from "eslint/config";
import vueParser from "vue-eslint-parser";

let autoImportGlobals = {};
if (fs.existsSync("./.eslintrc-auto-import.json")) {
  try {
    const content = JSON.parse(fs.readFileSync("./.eslintrc-auto-import.json", "utf-8"));
    autoImportGlobals = content.globals || {};
  }
  catch {
    // ignore
  }
}

export default antfu(
  {
    formatters: true,
    vue: true,
    stylistic: {
      indent: 2,
      semi: true,
      quotes: "double",
    },
  },

  globalIgnores([
    "**/dist/**",
    "**/dist-ssr/**",
    "**/coverage/**",
    "**/obj/**",
  ]),

  ...pluginOxlint.buildFromOxlintConfigFile(".oxlintrc.json"),

  {
    name: "app/auto-imports",
    languageOptions: {
      globals: {
        ...autoImportGlobals,
      },
    },
  },

  {
    name: "app/shadcn",
    files: ["**/*.{vue,ts,mts,tsx}"],
    languageOptions: {
      parser: vueParser,
      parserOptions: { parser: tsParser },
    },
    plugins: {
      shadcn,
    },
    rules: {
      "shadcn/no-arbitrary-values": "error",
      "shadcn/no-restyle": "error",
      "shadcn/no-raw-colors": "off",
      "shadcn/no-inline-styles": "error",
      "shadcn/no-unknown-classes": "error",
      "shadcn/require-static-classes": "error",
    },
  },

  {
    name: "app/files-to-lint",

    files: ["**/*.{vue,ts,mts,tsx}"],

    rules: {
      "vue/multi-word-component-names": "off",
      "antfu/if-newline": "off",
      "vue/max-attributes-per-line": ["error", {
        singleline: {
          max: 1,
        },
        multiline: {
          max: 1,
        },
      }],
      "ts/no-redeclare": "off",
      "vue/require-typed-ref": "error",
      "vue/block-order": [
        "error",
        { order: ["script", "template", "style"] },
      ],
      "vue/first-attribute-linebreak": [
        "error",
        {
          singleline: "beside",
          multiline: "below",
        },
      ],
      // Enforce new line between each tag
      "vue/padding-line-between-tags": [
        "error",
        [{
          blankLine: "always",
          prev: "*",
          next: "*",
        }],
      ],
      "vue/singleline-html-element-content-newline": [
        "error",
        {
          ignoreWhenNoAttributes: true,
          ignoreWhenEmpty: true,
        },
      ],
      // Enforce PascalCase for component names
      "vue/component-name-in-template-casing": [
        "error",
        "PascalCase",
        {
          registeredComponentsOnly: true,
          ignores: [],
        },
      ],
      "vue/custom-event-name-casing": [
        "error",
        "kebab-case",
        {
          ignores: [],
        },
      ],
    },
  },
);
