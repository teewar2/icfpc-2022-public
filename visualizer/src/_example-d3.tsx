import { useEffect } from "react";
import { randomNormal, select } from "d3";
import { D3ZoomEvent, zoom } from "d3-zoom";

const width = 512;
const height = 512;
const getPoints = () => {
  const randomX = randomNormal(width / 2, 80);
  const randomY = randomNormal(height / 2, 80);
  return Array.from({ length: 2000 }, () => [randomX(), randomY()] as Datum);
};
type Datum = [number, number];

function D3Example() {
  useEffect(() => {
    const root = select<HTMLElement, Datum>("#d3-example").attr("viewBox", [0, 0, width, height]);

    const viewport = zoom<HTMLElement, Datum>()
      .scaleExtent([1 / 10, 10])
      .on("zoom", zoomed);
    function zoomed({ transform }: D3ZoomEvent<HTMLElement, Datum>) {
      root.select("g").attr("transform", transform.toString());
    }
    root.call(viewport);

    const data = getPoints();
    root
      .select("g")
      .selectAll("circle")
      .data(data)
      .join("circle")
      .attr("r", 1)
      .attr("cx", (d) => d[0])
      .attr("cy", (d) => d[1]);
  }, []);
  return (
    <svg width="100vw" height="100vh" id="d3-example">
      <g></g>
    </svg>
  );
}

export default D3Example;
