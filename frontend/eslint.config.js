import pluginVue from "eslint-plugin-vue";

export default [
  ...pluginVue.configs["flat/essential"],
  {
    ignores: ["dist/**", "node_modules/**", "public/**"],
    rules: {
      "no-unused-vars": "error",
      "no-undef": "off",
      "vue/multi-word-component-names": "off",
    },
  },
];
