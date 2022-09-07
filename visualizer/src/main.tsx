import "./index.css";
import ReactDOM from "react-dom/client";
import App from "./App";
import D3Example from "./_example-d3";
import ThreeExample from "./_example-three";
import PixiExample from "./_example-pixi";

ReactDOM.createRoot(document.getElementById("root") as HTMLElement).render(<App />);
// ReactDOM.createRoot(document.getElementById("root") as HTMLElement).render(<D3Example />);
// ReactDOM.createRoot(document.getElementById("root") as HTMLElement).render(<ThreeExample />);
// ReactDOM.createRoot(document.getElementById("root") as HTMLElement).render(<PixiExample />);
