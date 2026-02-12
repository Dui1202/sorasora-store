-- CreateEnum
CREATE TYPE "ReferenceType" AS ENUM ('ORDER', 'PURCHASE', 'RETURN', 'TRANSFER', 'MANUAL');

-- AlterTable
ALTER TABLE "StockMovement" ADD COLUMN     "referenceType" "ReferenceType";

-- CreateIndex
CREATE INDEX "StockMovement_createdAt_idx" ON "StockMovement"("createdAt");
