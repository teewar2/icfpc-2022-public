import { filter, map } from "d3";
import React from "react";
import { Block } from "../../../contest-logic/Block";
import {
  InstructionType,
  Instruction,
  instructionToString,
} from "../../../contest-logic/Instruction";
import { Parser } from "../../../contest-logic/Parser";
import { Point } from "../../../contest-logic/Point";
import { Program } from "../../../contest-logic/Program";

export function getMousePoint(
  canvas: HTMLCanvasElement | null,
  event: React.MouseEvent<HTMLCanvasElement>
) {
  if (!canvas) return new Point([-1, -1]);
  const rect = canvas.getBoundingClientRect();
  return new Point([
    Math.trunc(event.clientX - rect.left),
    Math.trunc(rect.height - (event.clientY - rect.top)),
  ]);
}

export function canMergeBlocks(block1: Block, block2: Block) {
  const bottomToTop =
    (block1.bottomLeft.py === block2.topRight.py || block1.topRight.py === block2.bottomLeft.py) &&
    block1.bottomLeft.px === block2.bottomLeft.px &&
    block1.topRight.px === block2.topRight.px;
  const leftToRight =
    (block1.bottomLeft.px === block2.topRight.px || block1.topRight.px === block2.bottomLeft.px) &&
    block1.bottomLeft.py === block2.bottomLeft.py &&
    block1.topRight.py === block2.topRight.py;
  return bottomToTop || leftToRight;
}

export function mergeAllRectangles(length: number) {
  const size = Math.trunc(Math.sqrt(length));
  let id = length - 1;
  const commands: Instruction[] = [];
  let starting = 0;
  const columns = [];
  const merge = (b1: number, b2: number) => {
    commands.push({
      typ: InstructionType.MergeInstructionType,
      blockId1: String(b1),
      blockId2: String(b2),
    });
    id++;
  };
  for (let j = 0; j < size; j++) {
    merge(starting, starting + 1);
    for (let i = 1; i < size - 1; i++) {
      merge(id, size * j + i + 1);
    }
    columns.push(id);
    starting = size * (j + 1);
  }
  merge(columns[0], columns[1]);
  for (let i = 1; i < size - 1; i++) {
    merge(id, columns[i + 1]);
  }
  return commands;
}

export const shiftIdsBy = (lastId: number, code: string) => {
  const parser = new Parser();
  let result = parser.parse(code);
  if (result.typ === "error") {
    const [lineNumber, error] = result.result as [number, string];
    throw Error(`At ${lineNumber}, encountered: ${error}!`);
  }
  let program = result.result as Program;

  const shift = (blockId: string) => {
    const [head, ...tail] = blockId.split(".");
    const shifted = String(Number(head) + lastId);
    return [shifted, ...tail].join(".");
  };

  for (const instruction of program.instructions) {
    switch (instruction.typ) {
      case InstructionType.NopInstructionType:
      case InstructionType.CommentInstructionType: {
        break;
      }
      case InstructionType.ColorInstructionType:
      case InstructionType.PointCutInstructionType:
      case InstructionType.VerticalCutInstructionType:
      case InstructionType.VerticalCutInstructionType:
      case InstructionType.HorizontalCutInstructionType: {
        instruction.blockId = shift(instruction.blockId);
        break;
      }
      case InstructionType.SwapInstructionType:
      case InstructionType.MergeInstructionType: {
        instruction.blockId1 = shift(instruction.blockId1);
        instruction.blockId2 = shift(instruction.blockId2);
        break;
      }
    }
  }
  return program.instructions.map((i) => instructionToString(i)).join("\n");
};
