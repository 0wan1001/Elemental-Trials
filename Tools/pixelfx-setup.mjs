// pixelfx를 고정 커밋으로 External/pixelfx/ 에 받는다. 여러 번 실행해도 안전하다.
// 사용: node Tools/pixelfx-setup.mjs
// 버전을 올릴 때는 PIXELFX_COMMIT만 바꾸고 THIRD_PARTY_NOTICES.md의 커밋도 같이 고친다.
import { execFileSync } from "node:child_process";
import { cpSync, existsSync, mkdirSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const PIXELFX_REPO = "https://github.com/patch-gg/pixelfx.git";
const PIXELFX_COMMIT = "a84159688af3ca37a4be05a0d422b94451b2e24e"; // v0.3.0, 2026-10-31

const root = join(dirname(fileURLToPath(import.meta.url)), "..");
const dir = join(root, "External", "pixelfx");
const git = (...args) =>
  execFileSync("git", ["-C", dir, ...args], { encoding: "utf8", stdio: ["ignore", "pipe", "pipe"] }).trim();

if (!existsSync(join(dir, ".git"))) {
  mkdirSync(dir, { recursive: true });
  git("init", "-q");
  git("remote", "add", "origin", PIXELFX_REPO);
  // Patch Pixel 폰트(gallery/fonts)는 MIT가 아니고 쓰지 않으므로 gallery/는 받지 않는다.
  git("sparse-checkout", "set", "--no-cone", "/*", "!/gallery/");
}

let head = "";
try { head = git("rev-parse", "HEAD"); } catch { /* 아직 체크아웃 전 */ }

if (head !== PIXELFX_COMMIT) {
  git("fetch", "-q", "--depth", "1", "origin", PIXELFX_COMMIT);
  git("checkout", "-q", "--detach", PIXELFX_COMMIT);
}

// 이 프로젝트에서 만든 효과(Tools/pixelfx-fx/<분류>/*.js)를 체크아웃에 복사하고 목록을 다시 만든다
const customDir = join(root, "Tools", "pixelfx-fx");
if (existsSync(customDir)) {
  cpSync(customDir, join(dir, "src", "fx"), { recursive: true });
  execFileSync("node", ["tools/index.mjs"], { cwd: dir, stdio: "ignore" });
}

console.log(`pixelfx ready: ${dir} @ ${git("rev-parse", "--short", "HEAD")}`);
