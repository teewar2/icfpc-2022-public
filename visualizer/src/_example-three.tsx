import { useEffect } from "react";
import { randomNormal } from "d3";
import {
  DoubleSide,
  Mesh,
  MeshBasicMaterial,
  PerspectiveCamera,
  Scene,
  SphereGeometry,
  WebGLRenderer,
} from "three";
import { OrbitControls } from "three/examples/jsm/controls/OrbitControls";

const width = 512;
const height = 512;
const getPoints = () => {
  const randomX = randomNormal(width / 2, 80);
  const randomY = randomNormal(height / 2, 80);
  return Array.from(
    { length: 2000 },
    () => [-width / 2 + randomX(), -height / 2 + randomY()] as Datum
  );
};
type Datum = [number, number];

function ThreeExample() {
  useEffect(() => {
    const scene = new Scene();
    const camera = new PerspectiveCamera(45, window.innerWidth / window.innerHeight, 1, 10_000);
    camera.position.set(0, 0, 100);
    camera.lookAt(0, 0, 0);
    const renderer = new WebGLRenderer({ antialias: true });
    renderer.setSize(window.innerWidth, window.innerHeight);
    document.body.appendChild(renderer.domElement);
    const controls = new OrbitControls(camera, renderer.domElement);

    const points = getPoints();
    const geometry = new SphereGeometry(1);
    const material = new MeshBasicMaterial({ color: 0xffffff, side: DoubleSide });
    for (const point of points) {
      const p = new Mesh(geometry, material);
      p.position.x = point[0];
      p.position.y = point[1];
      scene.add(p);
    }

    let id = 0;
    function animate() {
      id = requestAnimationFrame(animate);
      controls.update();
      renderer.render(scene, camera);
    }
    animate();
    return () => {
      cancelAnimationFrame(id);
    };
  }, []);
  return null;
}

export default ThreeExample;
