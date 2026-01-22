export class Images {

  root: string = '/storee1lpoktk7/';
  limit = 13;


  public allImages() {
    const urls: string[] = [];
      for (let i = 1; i <= this.limit; i++) {
        urls.push(`${this.root}image${i}.jpg`);
      }

      return urls;
  }

  public parse(img: string) {
    if (img.includes("http")) {
      return img;
    }
    else {
      return `root/${img}`
    }
  }

}

const images = new Images();
export default images;
