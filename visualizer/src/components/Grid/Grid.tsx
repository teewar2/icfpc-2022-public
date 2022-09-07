import React, { MouseEventHandler, useEffect, useMemo, useRef, useState } from "react";

import { Point } from "../../contest-logic/Point";
import { getMousePoint } from "../Playground/shared/helpers";

const images = import.meta.glob("../../../../problems/*.png", { as: "url", eager: true });
const presets = import.meta.glob("../../../../problems/*.json", { eager: true });

type GridColumn = {
  width: number;
};

type GridRow = {
  height: number;
  columns: GridColumn[];
};

type Grid = {
  rows: GridRow[];
};

type SimpleGridRow = [number, number[]];
type SimpleGrid = SimpleGridRow[];

type Operation = "splitRow" | "splitColumn" | "merge";

const renderLine = (
  ctx: CanvasRenderingContext2D,
  fromX: number,
  fromY: number,
  toX: number,
  toY: number
) => {
  ctx.beginPath(); // Начинает новый путь
  ctx.moveTo(fromX, fromY); // Передвигает перо в точку (30, 50)
  ctx.lineTo(toX, toY); // Рисует линию до точки (150, 100)
  ctx.stroke();
};

const mapToSimple = (grid: Grid): SimpleGrid => {
  return grid.rows.map((row) => [row.height, row.columns.map((column) => column.width)]);
};
const mapFromSimple = (simpleGrid: SimpleGrid): Grid => {
  return simpleGrid.reduce<Grid>(
    (grid, row) => {
      const [height, columns] = row;
      grid.rows.push({
        height,
        columns: columns.map((column) => ({ width: column })),
      });

      return grid;
    },
    { rows: [] }
  );
};

const getRowOffset = (grid: Grid, rowIndex: number) => {
    return grid.rows.slice(0, rowIndex).reduce((sum, row) => sum + row.height, 0);
}

const getColumnOffset = (grid: Grid, rowIndex: number, columnIndex: number) => {
    return grid.rows[rowIndex].columns.slice(0, columnIndex).reduce((sum, column) => sum + column.width, 0);
}

const findRowColumnByPoint = (grid: Grid, point: Point) => {
  let insideRow: number | null = null;
  let insideColumn: number | null = null;
  let currentBottom = 0;
  for (let i = 0; i < grid.rows.length; i++) {
    const row = grid.rows[i];
    const rowHeight = row.height;
    const bottomY = currentBottom + rowHeight;
    if (point.py > currentBottom && point.py <= bottomY) {
      insideRow = i;
    }
    let currentLeft = 0;
    for (let j = 0; j < row.columns.length; j++) {
      const column = row.columns[j];
      const leftX = currentLeft + column.width;
      if (point.px > currentLeft && point.px <= leftX) {
        insideColumn = j;
        return [insideRow, insideColumn];
      }
      currentLeft += column.width;
    }
    if (insideRow) {
        return [insideRow, null];
    }
    currentBottom += rowHeight;
  }
  return [null, null];
};

export const Grid = (): JSX.Element => {
  const [width, setWidth] = useState(400);
  const [height, setHeight] = useState(400);
  const [expectedOpacity, _setExpectedOpacity] = useState(
    Number(sessionStorage.getItem("opacity")) ?? 0
  );
  const setExpectedOpacity = (opacity: number) => {
    sessionStorage.setItem("opacity", opacity.toString());
    _setExpectedOpacity(opacity);
  };
  const [exampleId, _setExampleId] = useState(
    sessionStorage.getItem("exampleId") ? Number(sessionStorage.getItem("exampleId")) : 1
  );
  const setExampleId = (exampleId: number) => {
    sessionStorage.setItem("exampleId", exampleId.toString());
    _setExampleId(exampleId);
  };

  const [grid, setGrid] = useState<Grid>({
    rows: [{ height, columns: [{ width }] }],
  });

  const canvasRef = useRef<HTMLCanvasElement | null>(null);

  const clearCanvas = () => {
    const canvas = canvasRef.current!;
    const context = canvas.getContext("2d")!;

    canvas.width = width;
    canvas.height = height;
    const imgData = context.getImageData(0, 0, canvas.width, canvas.height);
    imgData.data.forEach((value, index) => {
      imgData.data[index] = 255;
    });
  };

  const handleClickRenderCanvas = () => {
    clearCanvas();
    const canvas = canvasRef.current!;
    const context = canvas.getContext("2d")!;
    context.lineWidth = 1;
    context.strokeStyle = "rgba(102, 255, 0, 1)";
    let currentBottom = 0;
    for (let i = 0; i < grid.rows.length; i++) {
      const row = grid.rows[i];
      const rowHeight = row.height;
      const bottomY = currentBottom + rowHeight;
      renderLine(context, 0, width - bottomY, width, width - bottomY);
      let currentLeft = 0;
      for (let j = 0; j < row.columns.length; j++) {
        const column = row.columns[j];
        const leftX = currentLeft + column.width;
        renderLine(context, leftX, width - currentBottom, leftX, width - bottomY);
        currentLeft += column.width;
      }
      currentBottom += rowHeight;
    }
  };

  useEffect(() => {
    handleClickRenderCanvas();
  }, [grid]);

  const [operation, setOperation] = useState<Operation>("splitRow");

  const onCanvasClick: MouseEventHandler<HTMLCanvasElement> = (event) => {
    const canvas = canvasRef.current!;
    const context = canvas.getContext("2d")!;
    const point = getMousePoint(canvas, event);
    const [rowIndex, columnIndex] = findRowColumnByPoint(grid, point);
    console.log(rowIndex, columnIndex);
    const rowOffset = rowIndex ? getRowOffset(grid, rowIndex) : -1;
    const columnOffset = rowIndex && columnIndex ? getColumnOffset(grid, rowIndex, columnIndex) : -1;

    switch(operation) {
        case 'splitRow': {

        }
        case 'splitColumn': {

        }
        case 'merge': {

        }
    }
  };

  return (
    <div style={{ display: "flex", gap: 10, margin: 10 }}>
      <div style={{ display: "flex", flexDirection: "column", gap: 10 }}>
        <label>
          Example id
          <input
            type="number"
            value={exampleId}
            onChange={(event) => setExampleId(Number(event.target.value))}
          />
        </label>
        <label>
          width
          <br />
          <input
            type="text"
            value={width}
            onChange={(event) => setWidth(Number(event.target.value))}
          />
        </label>
        <label>
          height
          <br />
          <input
            type="text"
            value={height}
            onChange={(event) => setHeight(Number(event.target.value))}
          />
        </label>

        <button onClick={handleClickRenderCanvas}>Render grid</button>
        <textarea
          style={{ width: 600, height: 600 }}
          value={JSON.stringify(mapToSimple(grid), null, 4)}
          onChange={(event) => setGrid(mapFromSimple(JSON.parse(event.target.value)))}
        />
      </div>
      <div style={{ display: "flex", flexDirection: "column", gap: 10, position: "relative" }}>
        <canvas
          style={{ outline: "1px solid black" }}
          width={width}
          height={height}
          ref={canvasRef}
          onClick={onCanvasClick}
        />
        <img
          style={{
            position: "absolute",
            top: 0,
            left: 0,
            opacity: expectedOpacity,
            pointerEvents: "none",
          }}
          src={images[`../../../../problems/problem${exampleId}.png`]}
        />
        <input
          type="range"
          min={0}
          max={1}
          step={0.01}
          value={expectedOpacity}
          onChange={(event) => setExpectedOpacity(Number(event.target.value))}
        />
      </div>
      <div style={{ display: "flex", flexDirection: "column", gap: 10 }}>
        {["splitRow", "splitColumn", "merge"].map((operationType) => (
          <label key={operationType}>
            <input
              type="radio"
              checked={operationType === operation}
              onChange={(event) => setOperation(event.target.value)}
            />
            {operationType}
          </label>
        ))}
      </div>
    </div>
  );
};
