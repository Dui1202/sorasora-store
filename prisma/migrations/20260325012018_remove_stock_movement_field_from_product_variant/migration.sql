/*
  Warnings:

  - Added the required column `variantName` to the `StockMovement` table without a default value. This is not possible if the table is not empty.
  - Added the required column `variantSku` to the `StockMovement` table without a default value. This is not possible if the table is not empty.

*/
-- DropForeignKey
ALTER TABLE "StockMovement" DROP CONSTRAINT "StockMovement_variantId_fkey";

-- AlterTable
ALTER TABLE "StockMovement" ADD COLUMN     "variantName" TEXT NOT NULL,
ADD COLUMN     "variantSku" TEXT NOT NULL,
ALTER COLUMN "variantId" DROP NOT NULL;

-- AddForeignKey
ALTER TABLE "StockMovement" ADD CONSTRAINT "StockMovement_variantId_fkey" FOREIGN KEY ("variantId") REFERENCES "ProductVariant"("id") ON DELETE SET NULL ON UPDATE CASCADE;
