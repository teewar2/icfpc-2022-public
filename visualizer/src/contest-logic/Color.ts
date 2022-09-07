/* eslint-disable */

export class RGBA {
  r: number;

  g: number;

  b: number;

  a: number;

  constructor(rgba: [number, number, number, number] = [0, 0, 0, 0]) {
    [this.r, this.g, this.b, this.a] = rgba;
  }

  clone() {
    return new RGBA([this.r, this.g, this.b, this.a]);
  }

  isEqual(color: RGBA) {
    return this.r === color.r && this.g === color.g && this.b === color.b && this.a === color.a;
  }

  toString() {
    return `rgba(${this.r}, ${this.g}, ${this.b}, ${this.a / 255})`;
  }
}
