import _ from 'lodash';
import { Block } from "../../../contest-logic/Block";
import { Instruction, InstructionType } from '../../../contest-logic/Instruction';

export type GridColumn = {
    width: number;
  };

export  type GridRow = {
    height: number;
    columns: GridColumn[];
  };

export  type Grid = {
    rows: GridRow[];
  };

export  type SimpleGridRow = [number, number[]];
  export type SimpleGrid = SimpleGridRow[];


 export const mapToSimple = (grid: Grid): SimpleGrid => {
    return grid.rows.map((row) => [row.height, row.columns.map((column) => column.width)]);
  };
 export const mapFromSimple = (simpleGrid: SimpleGrid): Grid => {
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

export const getGridByBlocks = (blocks: Map<string, Block>): SimpleGrid => {
    try {
        const grid: Grid = { rows: [] };
        const blocksList = [...blocks.values()];
        const groups = _.groupBy(blocksList, (block: Block) => {
            return block.bottomLeft.py;
        })

        const rows = Object
            .keys(groups)
            .sort((a, b) => Number(a) - Number(b))
            .map(key => groups[key]);

        rows.forEach((row: Block[]) => {
            const columns: Block[] = [...row].sort((a, b) => Number(a.bottomLeft.px) - Number(b.bottomLeft.px));
            grid.rows.push({
                height: row[0].size.py,
                columns: columns.map(column => ({
                    width: column.size.px
                }))
            });
        });

        return mapToSimple(grid);
    } catch (error) {
        console.error(error);
        return [];
    }
};

export const getGridInstructions = (grid: SimpleGrid): Instruction[] => {
    const instructions: Instruction[] = [];

    let currentBlockId = '0';
    let heightOffset = 0;
    grid.forEach(([rowHeight, columns], index) => {
        let nextBlockId = '';
        if (index !== grid.length - 1) {
            instructions.push({
                typ: InstructionType.HorizontalCutInstructionType,
                blockId: currentBlockId,
                lineNumber: heightOffset + rowHeight
            });
            heightOffset += rowHeight;
            nextBlockId = `${currentBlockId}.1`;
            currentBlockId = `${currentBlockId}.0`;
        }
        let widthOffset = 0;
        columns.forEach((column, index) => {
            if (index !== columns.length - 1) {
                instructions.push({
                    typ: InstructionType.VerticalCutInstructionType,
                    blockId: currentBlockId,
                    lineNumber: widthOffset + column
                });
                widthOffset += column;
                currentBlockId = `${currentBlockId}.1`;
            }
        });
        currentBlockId = nextBlockId;
    });

    return instructions;
}
