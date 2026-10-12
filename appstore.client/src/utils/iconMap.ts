/**
 * Semantic Icon Mapping System.
 *
 * Decouples database entities (projects, categories, tags) from specific icon libraries.
 * The database stores abstract semantic words/keys (e.g. "rocket", "frontend", "database"),
 * and this map translates them to the active icon set (e.g. Lucide, Heroicons, Tabler).
 *
 * If you ever change the icon pack in the future, you ONLY need to update this file
 * without making any database migrations or data updates.
 */

/**
 * Union of all supported semantic icon words / keys.
 */
export type ProjectIcon
  // Development & Architecture
  = | "frontend"
    | "backend"
    | "fullstack"
    | "database"
    | "api"
    | "cloud"
    | "devops"
    | "mobile"
    | "ios"
    | "android"
    | "web"
    | "security"
    | "auth"
    | "ai"
    | "data"
    | "analytics"
    | "testing"
    | "qa"
    | "bug"
    | "code"
    | "terminal"
    | "tools"
    | "git"
    | "docker"
    | "infrastructure"
  // Design & Creative
    | "design"
    | "ui"
    | "ux"
    | "palette"
    | "figma"
    | "image"
    | "video"
    | "music"
    | "camera"
  // Business & Commerce
    | "business"
    | "finance"
    | "payment"
    | "billing"
    | "ecommerce"
    | "store"
    | "marketing"
    | "sales"
  // Project Management & Workflow
    | "rocket"
    | "launch"
    | "star"
    | "fire"
    | "target"
    | "task"
    | "kanban"
    | "calendar"
    | "clock"
    | "workflow"
    | "zap"
    | "flag"
    | "pin"
  // Collaboration & Social
    | "team"
    | "user"
    | "chat"
    | "mail"
    | "bell"
    | "heart"
    | "share"
  // Content & Docs
    | "docs"
    | "book"
    | "content"
    | "notes"
    | "archive"
    | "folder"
    | "link"
  // Infrastructure & Hardware
    | "server"
    | "cpu"
    | "harddrive"
    | "wifi"
    | "monitor"
  // General & Defaults
    | "settings"
    | "help"
    | "info"
    | "default";

/**
 * Primary mapping from semantic words to active icon library identifiers (currently Lucide).
 *
 * To switch icon libraries in the future (e.g. to Heroicons `i-heroicons:*` or Tabler `i-tabler:*`),
 * update the icon values in this dictionary.
 */
export const mapProjectIcons: Record<ProjectIcon, string> = {
  // Development & Architecture
  frontend: "i-lucide:layout",
  backend: "i-lucide:server",
  fullstack: "i-lucide:layers",
  database: "i-lucide:database",
  api: "i-lucide:webhook",
  cloud: "i-lucide:cloud",
  devops: "i-lucide:container",
  mobile: "i-lucide:smartphone",
  ios: "i-lucide:apple",
  android: "i-lucide:bot",
  web: "i-lucide:globe",
  security: "i-lucide:shield-check",
  auth: "i-lucide:lock",
  ai: "i-lucide:sparkles",
  data: "i-lucide:bar-chart-2",
  analytics: "i-lucide:line-chart",
  testing: "i-lucide:flask-conical",
  qa: "i-lucide:check-check",
  bug: "i-lucide:bug",
  code: "i-lucide:code-2",
  terminal: "i-lucide:terminal",
  tools: "i-lucide:wrench",
  git: "i-lucide:git-branch",
  docker: "i-lucide:box",
  infrastructure: "i-lucide:network",

  // Design & Creative
  design: "i-lucide:palette",
  ui: "i-lucide:layout-template",
  ux: "i-lucide:sparkles",
  palette: "i-lucide:palette",
  figma: "i-lucide:figma",
  image: "i-lucide:image",
  video: "i-lucide:video",
  music: "i-lucide:music",
  camera: "i-lucide:camera",

  // Business & Commerce
  business: "i-lucide:briefcase",
  finance: "i-lucide:dollar-sign",
  payment: "i-lucide:credit-card",
  billing: "i-lucide:receipt",
  ecommerce: "i-lucide:shopping-bag",
  store: "i-lucide:store",
  marketing: "i-lucide:megaphone",
  sales: "i-lucide:trending-up",

  // Project Management & Workflow
  rocket: "i-lucide:rocket",
  launch: "i-lucide:rocket",
  star: "i-lucide:star",
  fire: "i-lucide:flame",
  target: "i-lucide:target",
  task: "i-lucide:check-square",
  kanban: "i-lucide:kanban",
  calendar: "i-lucide:calendar",
  clock: "i-lucide:clock",
  workflow: "i-lucide:workflow",
  zap: "i-lucide:zap",
  flag: "i-lucide:flag",
  pin: "i-lucide:pin",

  // Collaboration & Social
  team: "i-lucide:users",
  user: "i-lucide:user",
  chat: "i-lucide:message-square",
  mail: "i-lucide:mail",
  bell: "i-lucide:bell",
  heart: "i-lucide:heart",
  share: "i-lucide:share-2",

  // Content & Docs
  docs: "i-lucide:file-text",
  book: "i-lucide:book-open",
  content: "i-lucide:file-code",
  notes: "i-lucide:notepad-text",
  archive: "i-lucide:archive",
  folder: "i-lucide:folder",
  link: "i-lucide:link",

  // Infrastructure & Hardware
  server: "i-lucide:server",
  cpu: "i-lucide:cpu",
  harddrive: "i-lucide:hard-drive",
  wifi: "i-lucide:wifi",
  monitor: "i-lucide:monitor",

  // General & Defaults
  settings: "i-lucide:settings",
  help: "i-lucide:help-circle",
  info: "i-lucide:info",
  default: "i-lucide:folder",
};

/**
 * Synonyms and alternate words mapping to canonical ProjectIcon keys.
 * Enables flexible DB strings like "fe", "client", "sql", "app", "dev" to resolve seamlessly.
 */
const SYNONYM_MAP: Record<string, ProjectIcon> = {
  // Frontend
  fe: "frontend",
  client: "frontend",
  spa: "frontend",
  vue: "frontend",
  react: "frontend",
  nuxt: "frontend",

  // Backend
  be: "backend",
  serverless: "backend",
  node: "backend",
  dotnet: "backend",
  csharp: "backend",

  // Database
  db: "database",
  sql: "database",
  postgres: "database",
  mysql: "database",
  mongodb: "database",
  redis: "database",

  // API
  rest: "api",
  graphql: "api",
  endpoint: "api",
  microservice: "api",

  // Mobile
  app: "mobile",
  phone: "mobile",

  // Cloud & Ops
  aws: "cloud",
  azure: "cloud",
  gcp: "cloud",
  kubernetes: "devops",
  k8s: "devops",
  ci: "devops",
  cd: "devops",

  // Security
  login: "auth",
  password: "security",
  admin: "settings",

  // Workflow & Tasks
  todo: "task",
  milestone: "target",
  goals: "target",
  speed: "zap",
  urgent: "fire",

  // Collaboration
  group: "team",
  people: "team",
  messages: "chat",
  notify: "bell",

  // Docs
  wiki: "docs",
  readme: "docs",
  documentation: "docs",
};

/**
 * Normalizes input key (trims, converts to lowercase, strips hyphens/underscores/spaces).
 */
function normalizeKey(key: string): string {
  return key.trim().toLowerCase().replace(/[\s\-_]+/g, "");
}

/**
 * Type guard checking if a string is a valid ProjectIcon key.
 */
export function isProjectIcon(key: string): key is ProjectIcon {
  return Object.hasOwn(mapProjectIcons, key);
}

/**
 * Resolves a semantic word or database string to its corresponding icon identifier.
 *
 * Handles:
 * - Direct matches (e.g. "rocket", "frontend")
 * - Case-insensitivity & formatting (e.g. "Front-End", "DATA_BASE")
 * - Common synonyms (e.g. "client", "fe", "sql", "app", "todo")
 * - Pre-formatted icon strings with a colon (e.g. "i-lucide:rocket", passes through)
 * - Safe fallback if no match is found (defaults to `mapProjectIcons.default`)
 *
 * @param iconOrWord - The semantic word, category name, or icon stored in DB
 * @param fallback - Optional custom fallback icon
 * @returns Fully qualified icon identifier ready for <UIcon>
 *
 * @example
 * getProjectIcon("frontend") // "i-lucide:layout"
 * getProjectIcon("db")       // "i-lucide:database"
 * getProjectIcon("unknown")  // "i-lucide:folder"
 */
export function getProjectIcon(
  iconOrWord?: string | null,
  fallback: string = mapProjectIcons.default,
): string {
  if (!iconOrWord || typeof iconOrWord !== "string") {
    return fallback;
  }

  const trimmed = iconOrWord.trim();
  if (!trimmed) {
    return fallback;
  }

  // 1. If it's already a full icon identifier (e.g. 'i-lucide:star' or 'lucide:star'), return directly
  if (trimmed.includes(":") || trimmed.startsWith("i-")) {
    return trimmed;
  }

  // 2. Direct exact match in mapProjectIcons
  const lower = trimmed.toLowerCase();
  if (isProjectIcon(lower)) {
    return mapProjectIcons[lower];
  }

  // 3. Normalized key match (stripping hyphens, underscores, spaces)
  const normalized = normalizeKey(trimmed);
  if (isProjectIcon(normalized)) {
    return mapProjectIcons[normalized];
  }

  // 4. Synonym match
  if (SYNONYM_MAP[lower]) {
    return mapProjectIcons[SYNONYM_MAP[lower]];
  }
  if (SYNONYM_MAP[normalized]) {
    return mapProjectIcons[SYNONYM_MAP[normalized]];
  }

  // 5. Partial word match (e.g. "Frontend Service" contains "frontend")
  for (const [key, icon] of Object.entries(mapProjectIcons)) {
    if (key !== "default" && (lower.includes(key) || normalized.includes(key))) {
      return icon;
    }
  }

  // 6. Safe fallback
  return fallback;
}

/**
 * Icon descriptor for UI selection lists, pickers, and previews.
 */
export interface ProjectIconOption {
  label: string;
  value: ProjectIcon;
  icon: string;
  category:
    | "Development"
    | "Design"
    | "Business"
    | "Management"
    | "Collaboration"
    | "Content"
    | "General";
}

/**
 * Curated list of icon options grouped by category for project creation/edit dialogs.
 */
export const projectIconOptions: ProjectIconOption[] = [
  // Development
  {
    label: "Frontend",
    value: "frontend",
    icon: mapProjectIcons.frontend,
    category: "Development",
  },
  { label: "Backend", value: "backend", icon: mapProjectIcons.backend, category: "Development" },
  { label: "Fullstack", value: "fullstack", icon: mapProjectIcons.fullstack, category: "Development" },
  { label: "Database", value: "database", icon: mapProjectIcons.database, category: "Development" },
  { label: "API / Webhook", value: "api", icon: mapProjectIcons.api, category: "Development" },
  { label: "Cloud", value: "cloud", icon: mapProjectIcons.cloud, category: "Development" },
  { label: "DevOps / Docker", value: "devops", icon: mapProjectIcons.devops, category: "Development" },
  { label: "Mobile App", value: "mobile", icon: mapProjectIcons.mobile, category: "Development" },
  { label: "Web / Browser", value: "web", icon: mapProjectIcons.web, category: "Development" },
  { label: "Security & Auth", value: "security", icon: mapProjectIcons.security, category: "Development" },
  { label: "AI & Intelligence", value: "ai", icon: mapProjectIcons.ai, category: "Development" },
  { label: "Testing & QA", value: "testing", icon: mapProjectIcons.testing, category: "Development" },
  { label: "Code", value: "code", icon: mapProjectIcons.code, category: "Development" },
  { label: "Terminal", value: "terminal", icon: mapProjectIcons.terminal, category: "Development" },

  // Design
  { label: "UI / UX Design", value: "design", icon: mapProjectIcons.design, category: "Design" },
  { label: "Color Palette", value: "palette", icon: mapProjectIcons.palette, category: "Design" },
  { label: "Figma", value: "figma", icon: mapProjectIcons.figma, category: "Design" },
  { label: "Media & Images", value: "image", icon: mapProjectIcons.image, category: "Design" },
  { label: "Video", value: "video", icon: mapProjectIcons.video, category: "Design" },

  // Business & Commerce
  { label: "Business", value: "business", icon: mapProjectIcons.business, category: "Business" },
  { label: "Finance & Money", value: "finance", icon: mapProjectIcons.finance, category: "Business" },
  { label: "Payments", value: "payment", icon: mapProjectIcons.payment, category: "Business" },
  { label: "E-Commerce", value: "ecommerce", icon: mapProjectIcons.ecommerce, category: "Business" },
  { label: "Marketing", value: "marketing", icon: mapProjectIcons.marketing, category: "Business" },
  { label: "Sales & Growth", value: "sales", icon: mapProjectIcons.sales, category: "Business" },

  // Management & Productivity
  { label: "Rocket / Launch", value: "rocket", icon: mapProjectIcons.rocket, category: "Management" },
  { label: "Featured Star", value: "star", icon: mapProjectIcons.star, category: "Management" },
  { label: "Trending / Priority", value: "fire", icon: mapProjectIcons.fire, category: "Management" },
  { label: "Goals & Targets", value: "target", icon: mapProjectIcons.target, category: "Management" },
  { label: "Tasks & Todos", value: "task", icon: mapProjectIcons.task, category: "Management" },
  { label: "Calendar & Dates", value: "calendar", icon: mapProjectIcons.calendar, category: "Management" },
  { label: "Workflow", value: "workflow", icon: mapProjectIcons.workflow, category: "Management" },
  { label: "Fast / Zap", value: "zap", icon: mapProjectIcons.zap, category: "Management" },

  // Collaboration
  { label: "Team & Members", value: "team", icon: mapProjectIcons.team, category: "Collaboration" },
  { label: "Chat & Discussions", value: "chat", icon: mapProjectIcons.chat, category: "Collaboration" },
  { label: "Mail & Contact", value: "mail", icon: mapProjectIcons.mail, category: "Collaboration" },
  { label: "Notifications", value: "bell", icon: mapProjectIcons.bell, category: "Collaboration" },

  // Content
  { label: "Documentation", value: "docs", icon: mapProjectIcons.docs, category: "Content" },
  { label: "Knowledge Base", value: "book", icon: mapProjectIcons.book, category: "Content" },
  { label: "Notes", value: "notes", icon: mapProjectIcons.notes, category: "Content" },
  { label: "Folder", value: "folder", icon: mapProjectIcons.folder, category: "Content" },

  // General
  { label: "Settings", value: "settings", icon: mapProjectIcons.settings, category: "General" },
  { label: "Default", value: "default", icon: mapProjectIcons.default, category: "General" },
];
