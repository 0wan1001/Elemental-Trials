// Elemental Trials: 보스 돌진 예고 띠 (반복, 32×16). 반투명 빨간 띠 위로 화살표(>)가 오른쪽으로 흘러간다.
// 가로로 이어 붙여(Tiled) 돌진 거리만큼 늘려 쓴다. 위·아래 테두리와 화살표가 칸 양 끝에서 이어지도록 그린다. 원점이 띠 왼쪽 가운데.
import { ramp, fit } from "../../core/index.js";

export const meta = {
  name: "ET charge warning",
  desc: "A tileable 32px strip of the boss charge warning: a translucent red band with bright edges and chevrons flowing right.",
  tags: ["boss", "warning", "tile", "elemental-trials"],
  size: [32, 16],
  origin: [0, 8],
  duration: 0.4,
  loop: true,
  fps: 12,
  bg: "dark",
  options: { palette: ["#5a0a0a", "#c81e1e", "#ff5040", "#ffb0a0"], seed: 36 },
};

export function draw(p, t, o) {
  const C = fit(ramp(o.palette), 4), shift = Math.round((t / meta.duration) * 16);
  for (let x = 0; x < 32; x++) {
    for (let y = -5; y <= 5; y++) p.dot(x, y, C[1], 0.3);
    p.dot(x, -6, C[2], 0.9);
    p.dot(x, 5, C[2], 0.9);
  }
  // 화살표 둘: 16px 간격, 한 바퀴에 16px 이동해 이어 붙여도 끊기지 않는다
  for (let k = 0; k < 2; k++) {
    for (let j = 0; j < 4; j++) {
      const bx = (k * 16 + shift + 64) % 32;
      for (const s of [-1, 1]) {
        const x = (bx - j + 32) % 32, y = s * j;
        p.dot(x, y, C[3], 1);
        p.dot((x + 31) % 32, y, C[2], 1);
      }
    }
  }
}
