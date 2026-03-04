/*
  Warnings:

  - You are about to drop the column `position` on the `ProductVariantImage` table. All the data in the column will be lost.
  - A unique constraint covering the columns `[variantId]` on the table `ProductVariantImage` will be added. If there are existing duplicate values, this will fail.

*/
-- AlterTable
ALTER TABLE "ProductVariant" ADD COLUMN     "position" INTEGER NOT NULL DEFAULT 0;

-- AlterTable
ALTER TABLE "ProductVariantImage" DROP COLUMN "position";

-- CreateTable
CREATE TABLE "ProductImage" (
    "id" TEXT NOT NULL,
    "productId" TEXT NOT NULL,
    "url" TEXT NOT NULL,
    "alt" TEXT,
    "position" INTEGER NOT NULL DEFAULT 0,
    "isCover" BOOLEAN NOT NULL DEFAULT false,

    CONSTRAINT "ProductImage_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "playing_with_neon" (
    "id" SERIAL NOT NULL,
    "name" TEXT NOT NULL,
    "value" REAL,

    CONSTRAINT "playing_with_neon_pkey" PRIMARY KEY ("id")
);

-- CreateIndex
CREATE INDEX "ProductImage_productId_idx" ON "ProductImage"("productId");

-- CreateIndex
CREATE UNIQUE INDEX "ProductVariantImage_variantId_key" ON "ProductVariantImage"("variantId");

-- AddForeignKey
ALTER TABLE "ProductImage" ADD CONSTRAINT "ProductImage_productId_fkey" FOREIGN KEY ("productId") REFERENCES "Product"("id") ON DELETE CASCADE ON UPDATE CASCADE;
