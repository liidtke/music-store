export class Images {

  root: string = 'https://ik.imagekit.io/storee1lpoktk7/';
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
      return `${this.root}${img}`
    }
  }

}

const images = new Images();
export default images;
