import { Injectable } from '@angular/core';
import { ProductDetail } from '../Models/product-detail';

@Injectable({
  providedIn: 'root'
})
export class ProductService {

  private _products: ProductDetail[];
  constructor() {
    this._products = [
      { productId: 1, brand: "Puma", name: "Suede black", price: 900, imgSrc: "../../assets/shoes.jpg" },
      { productId: 2, brand: "Jack & Jones", name: "Bomber Red", price: 1200, imgSrc: "../../assets/jacket.jpg" },
      { productId: 3, brand: "Adidas Originals", name: "White T shirt", price: 500, imgSrc: "../../assets/tshirt.jpg" },
    ]
  }

  getProducts(): ProductDetail[] {
    return this._products;
  }
}
