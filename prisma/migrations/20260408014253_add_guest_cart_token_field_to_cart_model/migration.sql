/*
  Warnings:

  - You are about to drop the column `sessionId` on the `Cart` table. All the data in the column will be lost.
  - A unique constraint covering the columns `[userId]` on the table `Cart` will be added. If there are existing duplicate values, this will fail.
  - A unique constraint covering the columns `[guestCartToken]` on the table `Cart` will be added. If there are existing duplicate values, this will fail.

*/
-- DropIndex
DROP INDEX "Cart_sessionId_idx";

-- AlterTable
ALTER TABLE "Cart" DROP COLUMN "sessionId",
ADD COLUMN     "guestCartToken" TEXT;

-- CreateIndex
CREATE UNIQUE INDEX "Cart_userId_key" ON "Cart"("userId");

-- CreateIndex
CREATE UNIQUE INDEX "Cart_guestCartToken_key" ON "Cart"("guestCartToken");

-- CreateIndex
CREATE INDEX "Cart_guestCartToken_idx" ON "Cart"("guestCartToken");
