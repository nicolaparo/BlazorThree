import { build } from "esbuild";
import { dirname, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const scriptDirectory = dirname(fileURLToPath(import.meta.url));
const repositoryRoot = resolve(scriptDirectory, "..");
const entryPoint = resolve(repositoryRoot, "src", "BlazorThree", "wwwroot", "blazorthree.js");
const outFile = resolve(repositoryRoot, "src", "BlazorThree", "wwwroot", "blazorthree.bundle.js");

await build({
  entryPoints: [entryPoint],
  outfile: outFile,
  bundle: true,
  format: "esm",
  target: "es2020",
  sourcemap: false,
  minify: true,
  legalComments: "none",
  charset: "utf8",
  logLevel: "info"
});
