/* eslint-disable */

import { Point } from "./Point";
import { RGBA } from "./Color";

export type Size = Point;
export enum BlockType {
  SimpleBlockType,
  ComplexBlockType,
  PngBlockType,
}
export type Block = SimpleBlock | ComplexBlock | PngBlock;

export class SimpleBlock {
  typ: BlockType.SimpleBlockType;

  id: string;

  bottomLeft: Point;

  topRight: Point;

  size: Size;

  color: RGBA;

  constructor(id: string, bottomLeft: Point, topRight: Point, color: RGBA) {
    this.typ = BlockType.SimpleBlockType;
    this.id = id;
    this.bottomLeft = bottomLeft;
    this.topRight = topRight;
    this.size = topRight.getDiff(bottomLeft);
    this.color = color;
    if (this.bottomLeft.px > this.topRight.px || this.bottomLeft.py > this.topRight.py) {
      throw Error("Invalid Block");
    }
  }

  getChildren() {
    return [this];
  }

  clone() {
    return new SimpleBlock(
      this.id,
      this.bottomLeft.clone(),
      this.topRight.clone(),
      this.color.clone()
    );
  }

  getSubBlock = (id: string, bottomLeft: Point, topRight: Point): SimpleBlock => {
    return new SimpleBlock(id, bottomLeft, topRight, this.color);
  };
}

export class ComplexBlock {
  typ: BlockType.ComplexBlockType;

  id: string;

  bottomLeft: Point;

  topRight: Point;

  size: Size;

  subBlocks: (SimpleBlock | PngBlock)[];

  constructor(
    id: string,
    bottomLeft: Point,
    topRight: Point,
    subBlocks: (SimpleBlock | PngBlock)[]
  ) {
    this.typ = BlockType.ComplexBlockType;
    this.id = id;
    this.bottomLeft = bottomLeft;
    this.topRight = topRight;
    this.size = topRight.getDiff(bottomLeft);
    this.subBlocks = subBlocks;
    if (this.bottomLeft.px > this.topRight.px || this.bottomLeft.py > this.topRight.py) {
    }
  }

  getChildren() {
    return this.subBlocks;
  }

  clone() {
    return new ComplexBlock(
      this.id,
      this.bottomLeft.clone(),
      this.topRight.clone(),
      this.subBlocks.map((b) => b.clone())
    );
  }
}

export class PngBlock {
  typ: BlockType.PngBlockType;

  id: string;

  bottomLeft: Point;

  topRight: Point;

  size: Size;

  colors: RGBA[];

  constructor(id: string, bottomLeft: Point, topRight: Point, colors: RGBA[]) {
    this.typ = BlockType.PngBlockType;
    this.id = id;
    this.bottomLeft = bottomLeft;
    this.topRight = topRight;
    this.size = topRight.getDiff(bottomLeft);
    this.colors = colors;
    if (this.bottomLeft.px > this.topRight.px || this.bottomLeft.py > this.topRight.py) {
    }
  }

  getChildren() {
    return [this];
  }

  clone() {
    return new PngBlock(
      this.id,
      this.bottomLeft.clone(),
      this.topRight.clone(),
      this.colors.slice()
    );
  }

  getColorsFrame = (bottomLeft: Point, size: Size): RGBA[] => {
    console.log(this.colors, bottomLeft, size);
    const offsetY = this.size.py - bottomLeft.py - size.py;
    const frameColors = [];
    for (let j = 0; j < size.py; j++) {
      for (let i = 0; i < size.px; i++) {
        frameColors.push(this.colors[(offsetY + j) * this.size.px + (bottomLeft.px + i)]);
      }
    }
    return frameColors;
  };

  getSubBlock = (id: string, bottomLeft: Point, topRight: Point): PngBlock => {
    return new PngBlock(
      id,
      bottomLeft,
      topRight,
      this.getColorsFrame(bottomLeft.getDiff(this.bottomLeft), topRight.getDiff(bottomLeft))
    );
  };
}
