// import { Block } from "../../../contest-logic/Block";
// import { RGBA } from "../../../contest-logic/Color";
// import { Instruction, InstructionType } from "../../../contest-logic/Instruction";
// import { Interpreter } from "../../../contest-logic/Interpreter";
// import { Point } from "../../../contest-logic/Point";

// const width = 400;
// const height = 400;
// const squareSize = 5;

// type Quadrant = 'top-left' | 'top-right' | 'bottom-left' | 'bottom-right';

// function getBlockByPoint(blocks: Map<string, Block>, point: Point) {
//     return [...blocks.values()].find(block => point.isInside(block.bottomLeft, block.topRight));
// }

// const isPointInsideBlock = (point: Point, block: Block) => {
//     return point.isInside(block.bottomLeft, block.topRight)
// }

// const getBlocks = (instructions: Instruction[]) => {
//     const interpreter = new Interpreter();
//     const result = interpreter.runInstructions(instructions);

//     return result.canvas.blocks;
// }

// const paintQuadrant = (instructions: Instruction[], point: Point, quadrant: Quadrant, color: RGBA) => {

// }

// export const makeCode = () => {
//     const instructions: Instruction[] = [
//         {
//             typ: InstructionType.PointCutInstructionType,
//             blockId: '0',
//             point: new Point([0, 0])
//         },
//         {
//             typ: InstructionType.ColorInstructionType,
//             blockId: '0.0',
//             color: new RGBA()
//         },
//         {
//             typ: InstructionType.ColorInstructionType,
//             blockId: '0.1',
//             color: new RGBA()
//         },
//         {
//             typ: InstructionType.ColorInstructionType,
//             blockId: '0.2',
//             color: new RGBA()
//         },
//         {
//             typ: InstructionType.ColorInstructionType,
//             blockId: '0.3',
//             color: new RGBA()
//         }
//     ];
//     while ()
//     for (let i = 0; i < width / squareSize; i++) {
//         for (let )
//     }
//     const interpreter = new Interpreter();
//     const blocks = getBlocks(instructions);
// };
