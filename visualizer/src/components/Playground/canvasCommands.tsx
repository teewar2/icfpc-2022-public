import { Instruction, instructionToString, InstructionType } from "../../contest-logic/Instruction";
import { MouseEvent } from "react";
import { Block } from "../../contest-logic/Block";
import { Point } from "../../contest-logic/Point";
import { canMergeBlocks, getMousePoint, mergeAllRectangles } from "./shared/helpers";
import { RGBA } from "../../contest-logic/Color";
import { Interpreter } from "../../contest-logic/Interpreter";
import _ from "lodash";

let prevPoint: Point | null = null;
let prevSelectedBlockId: string | undefined;

function getBlockByPoint(blocks: Map<string, Block>, point: Point) {
  return [...blocks.values()].find((block) => point.isInside(block.bottomLeft, block.topRight));
}

function getNewBlocks(interpreter: Interpreter, code: string, instructions: Instruction[]) {
  const result = interpreter.run(
    `${code}\n${instructions.map((i) => instructionToString(i)).join("\n")}`
  );

  return result.canvas.blocks;
}

function getCenterPoint(p1: Point, p2: Point) {
  const diffX = p2.px - p1.px;
  const diffY = p2.py - p1.py;

  return new Point([Math.trunc(p2.px - diffX / 2), Math.trunc(p2.py - diffY / 2)]);
}

function getMaxBlock(blocks: Map<string, Block>) {
  let maxSizeBlock: Block | null = null;
  for (const block of blocks.values()) {
    if (!maxSizeBlock) {
      maxSizeBlock = block;
      continue;
    }
    if (block.size.px * block.size.py > maxSizeBlock.size.px * maxSizeBlock.size.py) {
      maxSizeBlock = block;
    }
  }
  return maxSizeBlock!;
}

function getAdjustentBlock(blocks: Map<string, Block>, block: Block) {
  return [...blocks.values()].find((b) => {
    return canMergeBlocks(b, block);
  });
}

export function getClickInstruction(
  canvasRef: any,
  event: MouseEvent<HTMLCanvasElement>,
  instrument: InstructionType,
  blocks: Map<string, Block>,
  color: RGBA,
  code: string,
  interpreter: Interpreter
): Instruction | Instruction[] | undefined {
  if (!blocks) {
  }
  const point = getMousePoint(canvasRef.current, event);
  const currentBlock = getBlockByPoint(blocks, point);

  switch (instrument) {
    case InstructionType.HorizontalCutInstructionType: {
      return {
        // @ts-ignore
        blockId: currentBlock.id,
        typ: InstructionType.HorizontalCutInstructionType,
        lineNumber: point.py,
      } as Instruction;
    }
    case InstructionType.VerticalCutInstructionType: {
      return {
        // @ts-ignore
        blockId: currentBlock.id,
        typ: InstructionType.VerticalCutInstructionType,
        lineNumber: point.px,
      } as Instruction;
    }
    case InstructionType.PointCutInstructionType: {
      return {
        // @ts-ignore
        blockId: currentBlock.id,
        typ: InstructionType.PointCutInstructionType,
        point,
      } as Instruction;
    }
    case InstructionType.ColorInstructionType: {
      return {
        typ: InstructionType.ColorInstructionType,
        // @ts-ignore
        blockId: currentBlock.id,
        color,
      } as Instruction;
    }
    case InstructionType.SwapInstructionType: {
      if (!prevSelectedBlockId) {
        // @ts-ignore
        prevSelectedBlockId = currentBlock.id;
        return;
      }

      const res = {
        typ: InstructionType.SwapInstructionType,
        blockId1: prevSelectedBlockId,
        // @ts-ignore
        blockId2: currentBlock.id,
      } as Instruction;
      prevSelectedBlockId = undefined;
      return res;
    }
    case InstructionType.MergeInstructionType: {
      if (!prevSelectedBlockId) {
        // @ts-ignore
        prevSelectedBlockId = currentBlock.id;
        return;
      }

      const res = {
        typ: InstructionType.MergeInstructionType,
        blockId1: prevSelectedBlockId,
        // @ts-ignore
        blockId2: currentBlock.id,
      } as Instruction;
      prevSelectedBlockId = undefined;
      return res;
    }

    case InstructionType.Rectangle: {
      if (!prevPoint || !prevSelectedBlockId) {
        prevPoint = point;
        // @ts-ignore
        prevSelectedBlockId = currentBlock.id;
        return;
      }

      const instructions: Instruction[] = [];
      const firstCut: Instruction = {
        typ: InstructionType.PointCutInstructionType,
        blockId: prevSelectedBlockId,
        point: prevPoint,
      };
      instructions.push(firstCut);
      const blocks1 = getNewBlocks(interpreter, code, instructions);
      const block1 = getBlockByPoint(blocks1, point);
      const secondCut: Instruction = {
        typ: InstructionType.PointCutInstructionType,
        blockId: block1!.id,
        point,
      };
      instructions.push(secondCut);
      const blocks2 = getNewBlocks(interpreter, code, instructions);
      const block2 = getBlockByPoint(blocks2, getCenterPoint(prevPoint, point));
      const colorMove: Instruction = {
        typ: InstructionType.ColorInstructionType,
        color,
        blockId: block2!.id,
      };
      instructions.push(colorMove);

      prevPoint = null;
      prevSelectedBlockId = undefined;
      return instructions;
    }

    case InstructionType.ColorMerge: {
      const colorMove: Instruction = {
        typ: InstructionType.ColorInstructionType,
        color,
        blockId: currentBlock!.id,
      };
      const instructions: Instruction[] = [colorMove];
      const blocksList = [...blocks.values()];

      if (blocksList.length === 2) {
        instructions.push({
          typ: InstructionType.MergeInstructionType,
          blockId1: blocksList[0].id,
          blockId2: blocksList[1].id,
        });
        return instructions;
      }

      const maxBlock = getMaxBlock(blocks);
      const mergeBlock = getAdjustentBlock(blocks, maxBlock)!;
      instructions.push({
        typ: InstructionType.MergeInstructionType,
        blockId1: maxBlock.id,
        blockId2: mergeBlock.id,
      });
      const otherBlocks = blocksList.filter((b) => b.id !== maxBlock.id && b.id !== mergeBlock.id);
      instructions.push({
        typ: InstructionType.MergeInstructionType,
        blockId1: otherBlocks[0].id,
        blockId2: otherBlocks[1].id,
      });
      const mergedBlocks = [...getNewBlocks(interpreter, code, instructions).values()];
      instructions.push({
        typ: InstructionType.MergeInstructionType,
        blockId1: mergedBlocks[0].id,
        blockId2: mergedBlocks[1].id,
      });

      return instructions;
    }

    case InstructionType.AllAreMerged: {
      return mergeAllRectangles(interpreter.initialBlocks?.length ?? 1);
    }

    case InstructionType.LineMerge: {
      if (!prevSelectedBlockId) {
        // @ts-ignore
        prevSelectedBlockId = currentBlock.id;
        return;
      }

      const diff = blocks
        .get(currentBlock!.id)!
        .bottomLeft.getDiff(blocks.get(prevSelectedBlockId!)!.bottomLeft);

      console.log(currentBlock, prevSelectedBlockId);
      console.log(diff);

      if (diff.px > 0 && diff.py > 0) {
        alert("not a line");
        return;
      }

      const lineBlocks: Block[] = [...blocks.values()].filter((block: Block) => {
        return diff.px === 0
          ? block.bottomLeft.px === currentBlock?.bottomLeft.px
          : block.bottomLeft.py === currentBlock?.bottomLeft.py;
      });
      lineBlocks.sort((a, b) =>
        diff.px === 0 ? a.bottomLeft.py - b.bottomLeft.py : a.bottomLeft.px - b.bottomLeft.px
      );
      let startIndex = lineBlocks.findIndex((block) => block.id === currentBlock?.id);
      let endIndex = lineBlocks.findIndex((block) => block.id === prevSelectedBlockId);

      const maxId = [...blocks.values()].reduce((maxId, block) => {
        const mainId = Number(block.id.split(".")[0]);
        if (mainId > maxId) {
          return mainId;
        }
        return maxId;
      }, 0);

      if (startIndex > endIndex) {
        [startIndex, endIndex] = [endIndex, startIndex];
      }

      const instructions: Instruction[] = [];

      instructions.push({
        typ: InstructionType.MergeInstructionType,
        blockId1: lineBlocks[startIndex].id,
        blockId2: lineBlocks[startIndex + 1].id,
      });

      let currentId = maxId + 1;
      for (let i = startIndex + 1; i < endIndex; i++) {
        instructions.push({
          typ: InstructionType.MergeInstructionType,
          blockId1: String(currentId),
          blockId2: lineBlocks[i + 1].id,
        });
        currentId++;
      }

      prevSelectedBlockId = undefined;
      return instructions;
    }
  }
}
