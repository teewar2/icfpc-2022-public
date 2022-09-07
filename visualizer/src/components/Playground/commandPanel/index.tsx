import { FC, useState } from "react";
import { InstructionType } from "../../../contest-logic/Instruction";
import "./styles.css";
import { RGBA } from "../../../contest-logic/Color";

interface ICommandsPanel {
  instrument: InstructionType;
  colorRecord: Record<number, RGBA>;
  activeColorNumber: number;
  setActiveColorNumber(colorNum: number): void;
  setColor(value: RGBA, colorNum: number): void;
  setInstrument(value: InstructionType): void;
}

export const CommandsPanel: FC<ICommandsPanel> = ({
  instrument,
  colorRecord,
  activeColorNumber,
  setActiveColorNumber,
  setColor,
  setInstrument,
}) => {
  return (
    <div className={"commandPanel"}>
      <div style={{ display: "flex", gap: 10 }}>
        {[1, 2, 3].map((colorNum) => {
            const color = colorRecord[colorNum]

          return (
            <div key={colorNum}>
              <input
                id="color"
                type="radio"
                value={colorNum}
                name="color"
                checked={activeColorNumber === colorNum}
                onChange={(event) => setActiveColorNumber(Number(event.target.value))}
              />
              <input
                type="color"
                value={RGBAToHex(color)}
                onInput={(event) => setColor(hexToRGBA(event.target.value), colorNum)}
              />
              Shift + {colorNum}
            </div>
          );
        })}
      </div>
      {Object.values(InstructionType).map((value, key) => {
        const style = value === instrument ? "commandPanel__active-item" : "commandPanel__item";
        if (
          value === InstructionType.NopInstructionType ||
          value === InstructionType.CommentInstructionType
        ) {
          return null;
        }
        if (value === InstructionType.ColorInstructionType) {
          return (
            <div key={key} className={"commandPanel__item-with-color"}>
              <span className={style} onClick={() => setInstrument(value)}>
                {value}
              </span>
              Shift + C
            </div>
          );
        }
        return (
          <div key={key} className={"commandPanel__item-with-color"}>
            <span className={style} onClick={() => setInstrument(value)}>
              {value}
            </span>
            Shift + {value === InstructionType.ColorMerge ? "O" : value.slice(0, 1)}
          </div>
        );
      })}
    </div>
  );
};

function hexToRGBA(hex: string) {
  const r = parseInt(hex.slice(1, 3), 16);
  const g = parseInt(hex.slice(3, 5), 16);
  const b = parseInt(hex.slice(5, 7), 16);
  return new RGBA([r, g, b, 255]);
}

function RGBAToHex(rgba: RGBA) {
    return `#${toHex(rgba.r)}${toHex(rgba.g)}${toHex(rgba.b)}`;
}

function toHex(num: number) {
    const hex = num.toString(16);

    return hex.length === 1 ? `0${hex}` : hex
}
