import {
  defineConfig,
  presetAttributify,
  presetMini,
  presetIcons,
  presetTypography,
  transformerCompileClass,
} from "unocss";
import presetWind4 from "@unocss/preset-wind4";

const shortcuts = [
  {
    "card-title": "font-semibold text-lg",
    toaster:
      "max-w-xs bg-white border border-gray-200 rounded-md shadow-lg dark:bg-neutral-800 dark:border-neutral-700",
    "toaster-svg": "shrink-0 size-4 text-hs mt-0.5",
    error: "border-red hover:border-red-800",
    fieldset: "mb-4",
    "search-input":
      "w-full bg-transparent placeholder:text-slate-400 text-slate-700 text-sm border border-slate-200 h-44px rounded-sm pl-10 pr-3 py-2 transition duration-300 ease focus:outline-none focus:border-hs  shadow-sm focus:shadow hover:border-hs  transition duration-300 ease",
    input:
      "h-11 border py-2.5 sm:py-3 px-4 block w-full text-slate-700  border-slate-200 rounded-md sm:text-sm focus:outline-none focus:border-hs focus:shadow-md disabled:opacity-50 disabled:pointer-events-none dark:bg-neutral-900 dark:border-neutral-700 dark:text-neutral-400 dark:placeholder-neutral-500 dark:focus:ring-neutral-600 placeholder:text-slate-400 shadow-sm focus:shadow-md hover:border-hs transition duration-300 ease",
    "fieldset-legend": "text-sm font-medium text-slate-700",
    card: "rounded-md bg-white p-4",
    page: "bg-white rounded-md p-2 md:p-4",
    table: "min-w-full divide-y divide-gray-200 dark:divide-neutral-700",
    th: "px-6 py-3 text-start text-xs font-medium text-gray-500 uppercase dark:text-neutral-500",
    tr: "odd:bg-white even:bg-gray-100 dark:odd:bg-neutral-900 dark:even:bg-neutral-800",
    td: "px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-800 dark:text-neutral-200",
    select:
      "w-full h-11 bg-transparent placeholder:text-slate-400 text-slate-700 text-sm border border-slate-200 rounded-sm pl-3 pr-8 py-2 transition duration-300 ease focus:outline-none focus:border-hs hover:border-hs shadow-sm focus:shadow-md appearance-none cursor-pointer",
    "nav-item":
      "font-medium text-gray-600 hover:text-gray-400 focus:outline-hidden focus:text-gray-400 dark:text-neutral-400 dark:hover:text-neutral-500 dark:focus:text-neutral-500",
    "btn-cancel":
      "rounded bg-gray-100 px-4 py-2 text-sm font-medium text-gray-700 transition-colors hover:bg-gray-200",
    "btn-ok":
      "rounded bg-hs px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-hs-800",
    // "btn":"bg-gray-100 py-2 px-4  hover:text-white rounded-sm shadow-sm hover:bg-hs-500 focus:outline-none focus:ring-2 focus:ring-blue-400 focus:ring-opacity-75",
    "btn-primary":
      "appearance-none py-2 px-4 bg-hs-primary text-white font-semibold rounded-sm shadow-sm hover:bg-hs-800 focus:outline-none focus:ring-2 focus:ring-hs-secondary focus:ring-opacity-75",
    "btn-neutral":
      "appearance-none cursor-pointer py-2 px-4 bg-hs-neutral text-white font-semibold rounded-sm shadow-sm hover:bg-hs-neutral-600 focus:outline-none focus:ring-2 focus:ring-hs-secondary  focus:ring-opacity-75",
    "btn-secondary":
      "appearance-none cursor-pointer py-2 px-4 bg-hs-secondary text-white font-semibold rounded-sm shadow-sm hover:bg-hs-secondary-700 focus:outline-none focus:ring-2 focus:ring-hs-secondary  focus:ring-opacity-75",
    "btn-accent":
      "appearance-none cursor-pointer py-2 px-4 bg-hs-accent text-white font-semibold rounded-sm shadow-sm hover:bg-hs-accent-600 focus:outline-none focus:ring-2 focus:ring-hs-accent  focus:ring-opacity-75",
    "btn-negative":
      "appearance-none cursor-pointer py-2 px-4 bg-hs-negative text-white font-semibold rounded-sm shadow-sm hover:bg-hs-negative-600 focus:outline-none focus:ring-2 focus:ring-hs-negative  focus:ring-opacity-75",
    "btn-ghost":
      "appearance-none text-gray-700 cursor-pointer py-2 px-4 bg-white hover:text-white font-semibold rounded-sm shadow-sm hover:bg-hs focus:outline-none focus:ring-2 focus:ring-hs-secondary focus:ring-opacity-75",
    "btn-icon":
      "appearance-none p-0  bg-white hover:text-white font-semibold rounded-sm shadow-sm hover:bg-hs-600 focus:outline-none focus:ring-2 focus:ring-hs-secondary  focus:ring-opacity-75",
    "btn-sm":
      "py-2 px-3 inline-flex items-center gap-x-2 text-sm font-medium border border-transparent",
    "table-action":
      "cursor-pointer inline-flex items-center gap-x-2 text-sm font-semibold rounded-lg border border-transparent text-hs hover:text-hs-600 focus:outline-hidden focus:text-blue-800 disabled:opacity-50 disabled:pointer-events-none dark:text-white dark:hover:text-hs-400 dark:focus:text-hs-400",
  },
];

const theme = {
  colors: {
    hs: {
      neutral: {
        DEFAULT: "#7c909a", //
        "50": "#f8fafa",
        "100": "#f2f5f5",
        "200": "#e7eced",
        "300": "#d3dcdf",
        "400": "#bac8cb",
        "500": "#9dafb5",
        "600": "#7c909a",
        "700": "#73858e",
        "800": "#606f77",
        "900": "#505c62",
        "950": "#343c41",
      },
      accent: {
        DEFAULT: "#db5a58",
        '50': '#fdf3f3',
            '100': '#fbe5e5',
            '200': '#f8d1d0',
            '300': '#f2b0af',
            '400': '#e98280',
            '500': '#db5a58',
            '600': '#be3836',
            '700': '#a7302e',
            '800': '#8b2a29',
            '900': '#742928',
            '950': '#3e1211',
      },

      negative: {
        DEFAULT: "#f16c37", //orange something
        "50": "#fef4ee",
        "100": "#fde7d7",
        "200": "#facaae",
        "300": "#f7a57a",
        "400": "#f16c37",
        "500": "#ee5321",
        "600": "#e03a16",
        "700": "#b92a15",
        "800": "#942318",
        "900": "#771f17",
        "950": "#400d0a",
      },

      secondary: {
        DEFAULT: "#333c4d",
        "50": "#f6f7f9",
        "100": "#ebeef3",
        "200": "#d3d9e4",
        "300": "#acb9cd",
        "400": "#8093b0",
        "500": "#607697",
        "600": "#4c5e7d",
        "700": "#3e4d66",
        "800": "#364256",
        "900": "#333c4d",
        "950": "#202531",
      },

      primary: "#2b74c5",
      DEFAULT: "#2b74c5",
      '50': '#f2f7fd',
         '100': '#e4ecfa',
         '200': '#c3d8f4',
         '300': '#8fb9ea',
         '400': '#5395dd',
         '500': '#2b74c5',
         '600': '#1d5cac',
         '700': '#194a8b',
         '800': '#184074',
         '900': '#1a3760',
         '950': '#112340',
    },
  },
};

export default defineConfig({
  theme: theme,
  shortcuts: shortcuts,
  safelist: [
    "p-1",
    "p-2",
    "p-3",
    "text-center",
    "bg-red",
    "bg-white",
    "text-red",
    "text-hs-negative",
    "text-hs",
    "text-sm",
    "pb-8",
  ],
  presets: [
    presetIcons({
      /* options */
    }),
    presetAttributify({
      /* preset options */
    }),
    presetTypography(),
    presetWind4({
      preflights: { theme: true },
    }),
  ],
  content: {
    pipeline: {
      include: [
        // the default
        /\.(svelte|[jt]sx|vine.ts|mdx?|astro|elm|php|phtml|html)($|\?)/,
        // include js/ts files
        "src/**/*.{js,ts}",
      ],
      // exclude files
      // exclude: []
    },
  },
});
