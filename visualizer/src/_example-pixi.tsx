import { useEffect } from "react";
import * as PIXI from "pixi.js";
import { Viewport } from "pixi-viewport";
import { randomNormal } from "d3";

const width = 512;
const height = 512;
const getPoints = () => {
  const randomX = randomNormal(width / 2, 80);
  const randomY = randomNormal(height / 2, 80);
  return Array.from({ length: 2000 }, () => [randomX(), randomY()] as Datum);
};
type Datum = [number, number];

function PixiExample() {
  useEffect(() => {
    const app = new PIXI.Application({ width: width * 2, height: height * 2, antialias: true });
    document.body.appendChild(app.view);

    const viewport = new Viewport({
      screenWidth: window.innerWidth,
      screenHeight: window.innerHeight,
      worldWidth: width * 2,
      worldHeight: height * 2,
      interaction: app.renderer.plugins.interaction,
    });
    app.stage.addChild(viewport);
    viewport.drag().pinch().wheel().decelerate();

    const points = getPoints();
    for (const point of points) {
      const p = new PIXI.Graphics();
      p.beginFill(0xffffff);
      p.drawCircle(point[0], point[1], 2);
      p.endFill();
      viewport.addChild(p);
    }
  }, []);
  return null;
}

export default PixiExample;
