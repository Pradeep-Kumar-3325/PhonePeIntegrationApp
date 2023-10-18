export interface OrderDetail {
    orderId?: string;
    userId: number;
    customerName: string;
    orderAmount: number;
    mobileNumber?: string;
    email?: string;
}