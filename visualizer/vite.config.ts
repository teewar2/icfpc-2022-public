import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => ({
  plugins: [react()],
  server: { port: 8080 },
  define: {
    "process.env.NODE_ENV": JSON.stringify(mode),
  },
}));
