import { Canvas } from "../../contest-logic/Canvas";
import React, { useEffect, useMemo, useRef, useState } from "react";
import { RGBA } from "../../contest-logic/Color";
import { Interpreter, InterpreterResult } from "../../contest-logic/Interpreter";
import { Instruction, instructionToString, InstructionType } from "../../contest-logic/Instruction";
import { Painter } from "../../contest-logic/Painter";
import { RandomInstructionGenerator } from "../../contest-logic/RandomInstructionGenerator";
import { CommandsPanel } from "./commandPanel";

import { Point } from "../../contest-logic/Point";
import { Block, BlockType, PngBlock, SimpleBlock } from "../../contest-logic/Block";
import { getClickInstruction } from "./canvasCommands";
import { getMousePoint, mergeAllRectangles, shiftIdsBy } from "./shared/helpers";
import { SimilarityChecker } from "../../contest-logic/SimilarityCheck";
import { Parser } from "../../contest-logic/Parser";
import { getGridByBlocks, getGridInstructions } from "./shared/grid";

const images = import.meta.glob("../../../../problems/*.png", { as: "url", eager: true });
const presets = import.meta.glob("../../../../problems/*.json", { eager: true });

function getImageData(imgRef: HTMLImageElement) {
  const canvas = document.createElement("canvas");
  const context = canvas.getContext("2d");
  const height = (canvas.height = imgRef.naturalHeight || imgRef.offsetHeight || imgRef.height);
  const width = (canvas.width = imgRef.naturalWidth || imgRef.offsetWidth || imgRef.width);

  if (!context) {
    return null;
  }

  context.drawImage(imgRef, 0, 0);

  const data = context.getImageData(0, 0, width, height);

  return data.data;
}

export const Playground = (): JSX.Element => {
  const [isPlaying, setIsPlaying] = useState(false);
  const [playingLine, setPlayingLine] = useState(0);
  const [playSpeed, setPlaySpeed] = useState(200);

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
  const imgRef = useRef<HTMLImageElement | null>(null);

    const [useImage, setUseImage] = useState(false);

  const initialBlocks = useMemo(() => {
    if (useImage) {
        const imageData = getImageData(imgRef.current!);
        const colors = SimilarityChecker.bufferToFrame(imageData!);
        return [new PngBlock('0', new Point([0, 0]), new Point([400, 400]), colors)];
    }

    const preset = presets[`../../../../problems/problem${exampleId}.json`];
    const blocks: (SimpleBlock | PngBlock)[] | undefined =
      preset &&
      (preset as any).blocks.map(
        (block: {
          blockId: string;
          bottomLeft: [px: number, py: number] | undefined;
          topRight: [px: number, py: number] | undefined;
          color: [number, number, number, number] | undefined;
          colors: [number, number, number, number][];
        }) => {
          return block.colors
            ? new PngBlock(
                block.blockId,
                new Point(block.bottomLeft),
                new Point(block.topRight),
                block.colors.map(color => new RGBA(color))
            )
            : new SimpleBlock(
                block.blockId,
                new Point(block.bottomLeft),
                new Point(block.topRight),
                new RGBA(block.color)
              );
        }
      );
    return blocks;
  }, [exampleId, useImage]);
  const interpreter = useMemo(() => {
    return new Interpreter(initialBlocks);
  }, [initialBlocks]);
  const initialBlocksColors = useMemo(() => {
    if (!initialBlocks) {
      return;
    }

    return initialBlocks.reduce<RGBA[]>((colors, block) => {
      if (block.typ === BlockType.SimpleBlockType && colors.every((color) => !color.isEqual(block.color))) {
        colors.push(block.color);
      }

      return colors;
    }, []);
  }, [initialBlocks]);

  const setExampleId = (exampleId: number) => {
    sessionStorage.setItem("exampleId", exampleId.toString());
    _setExampleId(exampleId);
    handleSoftReset();
  };
  const [similarity, setSimilarity] = useState(0);
  const [oldTotal, setOldTotal] = useState(0);

  const [playgroundCode, _setPlaygroundCode] = useState(sessionStorage.getItem("code") ?? "");
  const setPlaygroundCode = (code: string) => {
    sessionStorage.setItem("code", code);
    _setPlaygroundCode(code);
  };
  const [instrument, setInstrument] = useState<InstructionType>(
    InstructionType.ColorInstructionType
  );
  const [interpretedResult, setInterpreterResult] = useState<InterpreterResult>(
    new InterpreterResult(new Canvas(400, 400, new RGBA([255, 255, 255, 255])), 0)
  );

  const [colorRecord, setColor] = useState<Record<number, RGBA>>({
    1: new RGBA([0, 0, 0, 255]),
    2: new RGBA([255, 255, 255, 255]),
    3: new RGBA([255, 0, 0, 255]),
  });
  const [chosenColor, setChosenColor] = useState(1);
  const color = colorRecord[chosenColor];

  const onSetColor = (color: RGBA, colorNumber: number) => {
    setColor((colorRecord) => ({
      ...colorRecord,
      [colorNumber]: color,
    }));
  };

  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const handlePlaygroundCode = (e: any) => {
    setPlaygroundCode(e.target.value as string);
  };
  const handleClickGenerateInstruction = () => {
    const result = interpreter.run(playgroundCode);

    const instruction = RandomInstructionGenerator.generateRandomInstruction(result.canvas);
    setPlaygroundCode(`${playgroundCode}\n${instructionToString(instruction)}`);
  };

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

  const handleClickRenderCanvas = (code: string) => {
    setOldTotal(interpretedResult.cost + similarity);
    clearCanvas();

    const result = interpreter.run(code);
    setInterpreterResult(result);

    console.log(result.canvas.blocks);

    const painter = new Painter();
    const renderedData = painter.draw(result.canvas);
    const canvas = canvasRef.current!;
    const context = canvas.getContext("2d")!;
    canvas.width = result.canvas.width;
    canvas.height = result.canvas.height;
    const imgData = context.getImageData(0, 0, canvas.width, canvas.height);
    renderedData.forEach((pixel: RGBA, index: number) => {
      imgData.data[index * 4] = pixel.r;
      imgData.data[index * 4 + 1] = pixel.g;
      imgData.data[index * 4 + 2] = pixel.b;
      imgData.data[index * 4 + 3] = pixel.a;
    });
    context.putImageData(imgData, 0, 0);
    drawBlocks(result);

    if (imgRef.current) {
      const expectedData = getImageData(imgRef.current)!;
      const expectedFrame = SimilarityChecker.bufferToFrame(expectedData);
      const actualFrame = SimilarityChecker.bufferToFrame(imgData.data);

      setSimilarity(SimilarityChecker.imageDiff(expectedFrame, actualFrame));
    }
  };
  const handleSoftReset = () => {
    clearCanvas();
    setInterpreterResult(
      new InterpreterResult(new Canvas(400, 400, new RGBA([255, 255, 255, 255])), 0)
    );
    setIsPlaying(false);
  };
  const handleReset = () => {
    setPlaygroundCode("");
    handleSoftReset();
  };
  const [drawBorder, setDrawBorder] = useState(true);
  const drawBlocks = (interpretedResult: InterpreterResult) => {
    if (!drawBorder) return;
    const context = canvasRef.current!.getContext("2d")!;
    const canvas = interpretedResult.canvas;
    const blocks = canvas.blocks;
    context.strokeStyle = "rgba(102, 255, 0, 1)";
    for (const [id, block] of blocks) {
      const frameTopLeft = new Point([block.bottomLeft.px, canvas.height - block.topRight.py]);
      const frameBottomRight = new Point([block.topRight.px, canvas.height - block.bottomLeft.py]);
      const sizeX = frameBottomRight.px - frameTopLeft.px;
      const sizeY = frameBottomRight.py - frameTopLeft.py;
      context.strokeRect(frameTopLeft.px, frameTopLeft.py, sizeX, sizeY);
    }
  };

  const [hoveringPoint, setHoveringPoint] = useState<Point | null>(null);
  const [hoveringBlock, setHoveringBlock] = useState<Block>();
  const onCanvasHover = (event: React.MouseEvent<HTMLCanvasElement>) => {
    const point = getMousePoint(canvasRef.current, event);
    const block = Array.from(interpretedResult?.canvas.blocks.values() ?? []).filter((b) =>
      point.isInside(b.bottomLeft, b.topRight)
    );
    setHoveringBlock(block.pop());
    setHoveringPoint(point);
  };

  useEffect(() => {
    const handler = (event: KeyboardEvent) => {
      if (event.key === "Enter" && event.shiftKey) {
        event.preventDefault();
        handleClickRenderCanvas(playgroundCode);
      }

      if (event.code === "KeyC" && event.shiftKey) {
        event.preventDefault();
        setInstrument(InstructionType.ColorInstructionType);
      }

      if (event.code === "KeyP" && event.shiftKey) {
        event.preventDefault();
        setInstrument(InstructionType.PointCutInstructionType);
      }

      if (event.code === "KeyV" && event.shiftKey) {
        event.preventDefault();
        setInstrument(InstructionType.VerticalCutInstructionType);
      }

      if (event.code === "KeyH" && event.shiftKey) {
        event.preventDefault();
        setInstrument(InstructionType.HorizontalCutInstructionType);
      }

      if (event.code === "KeyS" && event.shiftKey) {
        event.preventDefault();
        setInstrument(InstructionType.SwapInstructionType);
      }

      if (event.code === "KeyM" && event.shiftKey) {
        event.preventDefault();
        setInstrument(InstructionType.MergeInstructionType);
      }

      if (event.code === "KeyR" && event.shiftKey) {
        event.preventDefault();
        setInstrument(InstructionType.Rectangle);
      }

      if (event.code === "KeyO" && event.shiftKey) {
        event.preventDefault();
        setInstrument(InstructionType.ColorMerge);
      }

      if (event.code === "Digit1" && event.shiftKey) {
        event.preventDefault();
        setChosenColor(1);
      }

      if (event.code === "Digit2" && event.shiftKey) {
        event.preventDefault();
        setChosenColor(2);
      }

      if (event.code === "Digit3" && event.shiftKey) {
        event.preventDefault();
        setChosenColor(3);
      }

      if (
        (event.code === "ArrowLeft" ||
          event.code === "ArrowRight" ||
          event.code === "ArrowUp" ||
          event.code === "ArrowDown") &&
        event.altKey
      ) {
        event.preventDefault();
        const diffX = event.code === "ArrowLeft" ? -1 : event.code === "ArrowRight" ? 1 : 0;
        const diffY = event.code === "ArrowDown" ? -1 : event.code === "ArrowUp" ? 1 : 0;
        const parser = new Parser();
        const reversedLines = playgroundCode.split("\n").reverse();
        const lastInstrutionLineIndex = reversedLines.findIndex((line) => {
          const instruction = parser.parseLine(0, line).result as Instruction;
          return (
            instruction.typ === InstructionType.HorizontalCutInstructionType ||
            instruction.typ === InstructionType.PointCutInstructionType ||
            instruction.typ === InstructionType.VerticalCutInstructionType
          );
        });

        const lastInstrution = parser.parseLine(0, reversedLines[lastInstrutionLineIndex])
          .result as Instruction;
        if (lastInstrution?.typ === InstructionType.PointCutInstructionType) {
          lastInstrution.point = new Point([
            lastInstrution.point.px + diffX,
            lastInstrution.point.py + diffY,
          ]);
        }
        if (lastInstrution?.typ === InstructionType.HorizontalCutInstructionType) {
          if (!diffY) {
            return;
          }
          lastInstrution.lineNumber = lastInstrution.lineNumber + diffY;
        }
        if (lastInstrution?.typ === InstructionType.VerticalCutInstructionType) {
          if (!diffX) {
            return;
          }
          lastInstrution.lineNumber = lastInstrution.lineNumber + diffX;
        }
        reversedLines[lastInstrutionLineIndex] = instructionToString(lastInstrution);
        const newCode = reversedLines.reverse().join("\n");
        setPlaygroundCode(newCode);
        handleClickRenderCanvas(newCode);
      }

      if (event.code === "KeyZ" && (event.ctrlKey || event.metaKey)) {
        if (document.activeElement !== document.getElementById('codeTextArea')){
            event.preventDefault();
            const newCode = playgroundCode.split("\n").slice(0, -1).join("\n");
            setPlaygroundCode(newCode);
            handleClickRenderCanvas(newCode);
        }
      }

      if (event.code === "Equal" && event.shiftKey) {
        event.preventDefault();
        setExpectedOpacity(Math.max(Math.min(expectedOpacity + 0.5, 1), 0));
      }

      if (event.code === "Minus" && event.shiftKey) {
        event.preventDefault();
        setExpectedOpacity(Math.max(Math.min(expectedOpacity - 0.5, 1), 0));
      }

      if (event.code === "KeyL" && event.shiftKey) {
        event.preventDefault();
        setInstrument(InstructionType.LineMerge);
      }
    };

    document.addEventListener("keydown", handler);

    return () => document.removeEventListener("keydown", handler);
  }, [handleClickRenderCanvas, playgroundCode, expectedOpacity]);

  const onPlayClick = () => {
    setIsPlaying((isPlaying) => !isPlaying);
    setPlayingLine(0);
  };

  useEffect(() => {
    if (isPlaying) {
      const intervalId = setInterval(() => {
        setPlayingLine((playingLine) => playingLine + 1);
      }, playSpeed);

      return () => clearInterval(intervalId);
    }
  }, [isPlaying, playSpeed]);

  useEffect(() => {
    if (!isPlaying && playingLine === 0) {
      return;
    }
    try {
      handleClickRenderCanvas(playgroundCode.split("\n").slice(0, playingLine).join("\n"));
    } catch (e) {
      console.log(e);
    }
  }, [handleClickRenderCanvas, isPlaying, playingLine, playgroundCode]);

  const onMakeCode = () => {
    const code = "";

    setPlaygroundCode(code);
  };

  const total = interpretedResult?.cost + similarity;
  const diff = total - oldTotal;

  const [shiftBy, setShiftBy] = useState("");

  const downloadUrl = () => {
    const blob = new Blob([playgroundCode], { type: "text/plain; charset=utf-8" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.setAttribute("href", url);
    a.setAttribute("download", `problem-${exampleId}-${total}_${Date.now()}.txt`);
    a.click();
    a.remove();
    setTimeout(() => {
      URL.revokeObjectURL(url);
    }, 10 * 1000);
  };

  const gridCodeRef = useRef(null);
  const grid = useMemo(
    () => interpretedResult?.canvas.blocks && getGridByBlocks(interpretedResult.canvas.blocks),
    [interpretedResult]
  );
  const onGenerateByGrid = () => {
    const gridCode = gridCodeRef.current!.value;
    try {
      const grid = JSON.parse(gridCode);
      const instructions = getGridInstructions(grid);
      setPlaygroundCode(instructions.map((i) => instructionToString(i)).join("\n"));
    } catch (error) {
      console.error(error);
    }
  };

  return (
    <div
      style={{
        display: "flex",
        maxWidth: "100vw",
        gap: "20px",
        marginTop: 10,
      }}
    >
      <div>
        <div>
          <div style={{ fontSize: 12, lineHeight: "14px", width: 300 }}>
            Shift + Enter – rerender canvas
            <br /> Alt + Arrows – shift last command point
            <br /> Command + Z – remove last command
            <br />
            Shift + "+", Shift + "-" – change example opacity
          </div>
          <label>
            Example id
            <input
              type="number"
              value={exampleId}
              onChange={(event) => setExampleId(Number(event.target.value))}
            />
          </label>
        </div>
        <div>
          <button onClick={handleClickGenerateInstruction}>Generate Instruction</button>
          <button onClick={() => handleClickRenderCanvas(playgroundCode)}>Render Canvas</button>
          <button onClick={handleReset}>Reset</button>
          <button onClick={() => setUseImage(useImage => !useImage)}>Use image</button>
          {/* <button onClick={onMakeCode}>Make code</button> */}
          <label>
            <input
              type="checkbox"
              checked={drawBorder}
              onChange={(e) => setDrawBorder(e.target.checked)}
            />
            border
          </label>
        </div>
        <div>
          <label>
            Play speed
            <input
              type="number"
              value={playSpeed}
              onChange={(event) => setPlaySpeed(Number(event.target.value))}
            />
          </label>
          <button onClick={onPlayClick}>{isPlaying ? "Stop" : "Play"}</button>
          <button onClick={() => setPlayingLine(playingLine + 1)}>{"Next Step"}</button>
          <button onClick={() => setPlayingLine(playingLine - 1)}>{"Prev Step"}</button>
        </div>
        <div>
          <div>
            <label>
              width
              <br />
              <input
                type="text"
                value={width}
                onChange={(event) => setWidth(Number(event.target.value))}
              />
            </label>
            <br />
            <label>
              height
              <br />
              <input
                type="text"
                value={height}
                onChange={(event) => setHeight(Number(event.target.value))}
              />
            </label>
            <br />
            <div>
              <div>
                code{" "}
                <button
                  onClick={() => setPlaygroundCode(shiftIdsBy(Number(shiftBy), playgroundCode))}
                >
                  shift by:
                </button>{" "}
                <input type="text" value={shiftBy} onChange={(e) => setShiftBy(e.target.value)} />
                <button onClick={() => downloadUrl()}>download</button>
              </div>
              <div
                style={{
                  display: "flex",
                  fontSize: "14px",
                  lineHeight: "18px",
                }}
              >
                <textarea
                    id={'codeTextArea'}
                  style={{
                    width: "500px",
                    height: "400px",
                    fontSize: "14px",
                    lineHeight: "18px",
                  }}
                  placeholder="Code to be submitted"
                  value={playgroundCode}
                  onChange={handlePlaygroundCode}
                />
                <div style={{ width: "5ch" }}>
                  {interpretedResult?.instructionCosts.map((cost, index) => (
                    <div key={index} onClick={() => setPlayingLine(index)}>
                      {cost}
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div
        style={{
          flexShrink: 0,
          display: "flex",
          flexDirection: "column",
          gap: 10,
          position: "relative",
          maxWidth: width,
        }}
      >
        <canvas
          style={{ outline: "1px solid black" }}
          width={width}
          height={height}
          ref={canvasRef}
          onClick={(event) => {
            const instruction = getClickInstruction(
              canvasRef,
              event,
              instrument,
              interpretedResult.canvas.blocks,
              color,
              playgroundCode,
              interpreter
            );
            if (instruction) {
              const code = Array.isArray(instruction)
                ? `${playgroundCode}\n${instruction.map((i) => instructionToString(i)).join("\n")}`
                : `${playgroundCode}\n${instructionToString(instruction)}`;
              setPlaygroundCode(code);
              handleClickRenderCanvas(code);
            }
          }}
          onMouseMove={onCanvasHover}
          onMouseOver={onCanvasHover}
          onMouseLeave={() => {
            setHoveringBlock(undefined);
            setHoveringPoint(null);
          }}
        />
        <img
          ref={imgRef}
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
        <div>
          Hovering: {hoveringPoint ? `(${hoveringPoint.px},${hoveringPoint.py})` : ""}{" "}
          {hoveringBlock &&
            [
              hoveringBlock.id,
              `size: ${hoveringBlock.size.px}x${hoveringBlock.size.py}`,
              `bottomLeft: ${hoveringBlock.bottomLeft.px} ${hoveringBlock.bottomLeft.py}`,
              hoveringBlock.typ === BlockType.SimpleBlockType
                ? `color ${(hoveringBlock as SimpleBlock).color}`
                : "Complex block",
            ].join("; ")}
        </div>
        <div>Cost: {interpretedResult?.cost}</div>
        <div>Similarity: {similarity}</div>
        <div>Total: {total}</div>
        <div style={{ color: diff > 0 ? "red" : "green" }}>Diff: {diff}</div>
        {playgroundCode.split("\n")[playingLine] && (
          <div>Playing command: {playgroundCode.split("\n")[playingLine]}</div>
        )}
        {initialBlocksColors && (
          <div>
            Initial colors:
            {initialBlocksColors.map((color) => {
              return (
                <div key={color.toString()}>
                  <span
                    style={{
                      display: "inline-block",
                      width: 10,
                      height: 10,
                      background: color.toString(),
                      border: "1px solid white",
                      outline: "1px solid black",
                      marginRight: 5,
                    }}
                  ></span>
                  {color.toString()}
                </div>
              );
            })}
          </div>
        )}
        <details>
          <summary>grid</summary>
          <textarea
            ref={gridCodeRef}
            style={{ width: 400, height: 200 }}
            value={JSON.stringify(grid, null, 4)}
          />
          <button onClick={onGenerateByGrid}>Generate code</button>
        </details>
      </div>
      <CommandsPanel
        colorRecord={colorRecord}
        setColor={onSetColor}
        activeColorNumber={chosenColor}
        setActiveColorNumber={setChosenColor}
        instrument={instrument}
        setInstrument={setInstrument}
      />
    </div>
  );
};
