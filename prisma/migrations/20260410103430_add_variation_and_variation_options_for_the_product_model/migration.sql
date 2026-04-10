-- CreateTable
CREATE TABLE "Variation" (
    "id" TEXT NOT NULL,
    "productId" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "position" INTEGER NOT NULL DEFAULT 0,
    "createdAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedAt" TIMESTAMP(3) NOT NULL,

    CONSTRAINT "Variation_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "VariationOption" (
    "id" TEXT NOT NULL,
    "variationId" TEXT NOT NULL,
    "value" TEXT NOT NULL,
    "position" INTEGER NOT NULL DEFAULT 0,
    "createdAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedAt" TIMESTAMP(3) NOT NULL,

    CONSTRAINT "VariationOption_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "ProductVariantOption" (
    "productVariantId" TEXT NOT NULL,
    "variationId" TEXT NOT NULL,
    "variationOptionId" TEXT NOT NULL,

    CONSTRAINT "ProductVariantOption_pkey" PRIMARY KEY ("productVariantId","variationId")
);

-- CreateIndex
CREATE INDEX "Variation_productId_idx" ON "Variation"("productId");

-- CreateIndex
CREATE INDEX "Variation_productId_position_idx" ON "Variation"("productId", "position");

-- CreateIndex
CREATE UNIQUE INDEX "Variation_productId_name_key" ON "Variation"("productId", "name");

-- CreateIndex
CREATE INDEX "VariationOption_variationId_idx" ON "VariationOption"("variationId");

-- CreateIndex
CREATE INDEX "VariationOption_variationId_position_idx" ON "VariationOption"("variationId", "position");

-- CreateIndex
CREATE UNIQUE INDEX "VariationOption_variationId_value_key" ON "VariationOption"("variationId", "value");

-- CreateIndex
CREATE UNIQUE INDEX "VariationOption_id_variationId_key" ON "VariationOption"("id", "variationId");

-- CreateIndex
CREATE INDEX "ProductVariantOption_variationId_idx" ON "ProductVariantOption"("variationId");

-- CreateIndex
CREATE INDEX "ProductVariantOption_variationOptionId_idx" ON "ProductVariantOption"("variationOptionId");

-- CreateIndex
CREATE UNIQUE INDEX "ProductVariantOption_productVariantId_variationOptionId_key" ON "ProductVariantOption"("productVariantId", "variationOptionId");

-- AddForeignKey
ALTER TABLE "Variation" ADD CONSTRAINT "Variation_productId_fkey" FOREIGN KEY ("productId") REFERENCES "Product"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "VariationOption" ADD CONSTRAINT "VariationOption_variationId_fkey" FOREIGN KEY ("variationId") REFERENCES "Variation"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "ProductVariantOption" ADD CONSTRAINT "ProductVariantOption_productVariantId_fkey" FOREIGN KEY ("productVariantId") REFERENCES "ProductVariant"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "ProductVariantOption" ADD CONSTRAINT "ProductVariantOption_variationId_fkey" FOREIGN KEY ("variationId") REFERENCES "Variation"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "ProductVariantOption" ADD CONSTRAINT "ProductVariantOption_variationOptionId_variationId_fkey" FOREIGN KEY ("variationOptionId", "variationId") REFERENCES "VariationOption"("id", "variationId") ON DELETE CASCADE ON UPDATE CASCADE;
