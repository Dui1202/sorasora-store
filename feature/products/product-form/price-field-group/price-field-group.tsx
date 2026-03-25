import { Button } from "@/components/ui/button";
import {
  Field,
  FieldDescription,
  FieldGroup,
  FieldLabel,
} from "@/components/ui/field";
import {
  InputGroup,
  InputGroupAddon,
  InputGroupInput,
} from "@/components/ui/input-group";
import { Input } from "@/components/ui/input";
import { Plus } from "lucide-react";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useFormContext, useWatch } from "react-hook-form";
import {
  handleDigitOnlyKeyDown,
  handleDigitOnlyPaste,
  toDigitsOnly,
} from "./number-guard";
import VariationFieldGroup from "./variation-field-group/variation-field-group";
import { calculateBreakEvenPrice } from "./pricing/calculate-break-even-price";
import { COUNTRY_CONFIG } from "./pricing/config";
import {
  createEmptyVariation,
  useVariation,
} from "./variation-field-group/use-variation";
import VariationPriceTableField from "./variation-price-table-field/variation-price-table-field";
import { parseExistingVariantsForEditor } from "./variation-price-table-field/variation-price-table-field-utils";
import { VariantDraftSeed } from "./variation-price-table-field/use-variant-draft-rows";
import {
  PriceFormInputData,
  PriceFormMode,
  PriceFormVariantInput,
} from "./types";

type PriceFieldGroupProps = {
  mode: PriceFormMode;
};

type InitialVariationEditorState = {
  initialVariations: ReturnType<typeof useVariation>["variations"];
  initialDraftSeeds: VariantDraftSeed[];
  parseError: string | null;
};

const DEFAULT_EDITOR_STATE: InitialVariationEditorState = {
  initialVariations: [],
  initialDraftSeeds: [],
  parseError: null,
};

const toInitialVariationState = (
  mode: PriceFormMode,
  hasVariation: boolean,
  variants: PriceFormVariantInput[],
): InitialVariationEditorState => {
  if (mode !== "update" || !hasVariation) {
    return DEFAULT_EDITOR_STATE;
  }

  const parsed = parseExistingVariantsForEditor(variants);

  if (!parsed.success) {
    return {
      initialVariations: [createEmptyVariation()],
      initialDraftSeeds: [],
      parseError: parsed.error,
    };
  }

  return {
    initialVariations: parsed.data.variations,
    initialDraftSeeds: parsed.data.seeds,
    parseError: null,
  };
};

const PriceFieldGroup = ({ mode }: PriceFieldGroupProps) => {
  const {
    control,
    setValue,
    register,
    getValues,
    setError,
    clearErrors,
    formState,
  } = useFormContext<PriceFormInputData>();
  const { errors } = formState;

  const initialVariationState = useMemo<InitialVariationEditorState>(() => {
    const hasVariation = getValues("hasVariation") ?? false;
    const variants = getValues("variants") ?? [];
    return toInitialVariationState(mode, hasVariation, variants);
  }, [getValues, mode]);

  const variationState = useVariation(initialVariationState.initialVariations);
  const { addVariation, isVariationEnable, variations } = variationState;
  const [initialDraftSeeds, setInitialDraftSeeds] = useState<
    VariantDraftSeed[]
  >(initialVariationState.initialDraftSeeds);
  const previousIsVariationEnableRef = useRef(isVariationEnable);

  const supplierCountry = useWatch({
    control,
    name: "supplierCountry",
  });

  const baseCostPriceField = register("baseCostPrice", {
    setValueAs: (value) => {
      const digits = toDigitsOnly(String(value ?? ""));
      return digits === "" ? undefined : Number(digits);
    },
  });

  const baseSellingPriceField = register("baseSellingPrice", {
    setValueAs: (value) => {
      const digits = toDigitsOnly(String(value ?? ""));
      return digits === "" ? undefined : Number(digits);
    },
  });
  const baseStockField = register("baseStock.quantity", {
    setValueAs: (value) => {
      const digits = toDigitsOnly(String(value ?? ""));
      return digits === "" ? undefined : Number(digits);
    },
  });

  useEffect(() => {
    if (!initialVariationState.parseError) {
      return;
    }

    setError("variants", {
      type: "manual",
      message: initialVariationState.parseError,
    });
  }, [initialVariationState.parseError, setError]);

  useEffect(() => {
    const wasVariationEnabled = previousIsVariationEnableRef.current;
    previousIsVariationEnableRef.current = isVariationEnable;

    if (wasVariationEnabled === isVariationEnable) {
      return;
    }

    if (isVariationEnable) {
      setValue("hasVariation", true, {
        shouldDirty: true,
        shouldValidate: true,
      });
      setValue("baseSku", undefined, {
        shouldDirty: true,
        shouldValidate: true,
      });
      setValue("baseCostPrice", undefined, {
        shouldDirty: true,
        shouldValidate: true,
      });
      setValue("baseSellingPrice", undefined, {
        shouldDirty: true,
        shouldValidate: true,
      });
      setValue("baseStock", undefined, {
        shouldDirty: true,
        shouldValidate: true,
      });
      return;
    }

    setValue("hasVariation", false, {
      shouldDirty: true,
      shouldValidate: true,
    });
    setValue("variants", [], {
      shouldDirty: true,
      shouldValidate: true,
    });
    setValue("baseSku", undefined, {
      shouldDirty: true,
      shouldValidate: true,
    });
    setValue("baseCostPrice", undefined, {
      shouldDirty: true,
      shouldValidate: true,
    });
    setValue("baseSellingPrice", undefined, {
      shouldDirty: true,
      shouldValidate: true,
    });
    setValue("baseStock", undefined, {
      shouldDirty: true,
      shouldValidate: true,
    });
  }, [isVariationEnable, setValue]);

  const handleVariantsChange = useCallback(
    (variants: PriceFormVariantInput[]) => {
      setValue("variants", variants, {
        shouldDirty: true,
        shouldValidate: true,
      });

      if (variants.length > 0) {
        clearErrors("variants");
      }
    },
    [clearErrors, setValue],
  );

  const handleEnableVariation = useCallback(() => {
    setInitialDraftSeeds([]);
    clearErrors("variants");
    addVariation();
  }, [addVariation, clearErrors]);

  const [breakEvenPrice, setBreakEvenPrice] = useState(0);
  const costCurrencyCode = supplierCountry
    ? COUNTRY_CONFIG[supplierCountry].currencyCode
    : "Local";
  const variantsErrorMessage =
    typeof errors.variants?.message === "string"
      ? errors.variants.message
      : undefined;

  return (
    <>
      <FieldGroup>
        <Field>
          <FieldLabel>Variations</FieldLabel>
          {variantsErrorMessage && (
            <FieldDescription className="text-destructive">
              {variantsErrorMessage}
            </FieldDescription>
          )}
          {!isVariationEnable && (
            <div>
              <Button onClick={handleEnableVariation} type="button">
                <Plus />
                Add Variation
              </Button>
            </div>
          )}
        </Field>
      </FieldGroup>
      {!isVariationEnable && (
        <>
          <FieldGroup className="flex flex-row">
            <Field>
              <FieldLabel>Original Price</FieldLabel>
              <FieldDescription>
                Orginal price in the supplier country&apos;s currency
              </FieldDescription>
              <InputGroup>
                <InputGroupAddon>{costCurrencyCode}</InputGroupAddon>
                <InputGroupInput
                  type="text"
                  inputMode="numeric"
                  pattern="[0-9]*"
                  placeholder="Orginal price..."
                  {...baseCostPriceField}
                  onKeyDown={handleDigitOnlyKeyDown}
                  onPaste={handleDigitOnlyPaste}
                  onChange={(event) => {
                    baseCostPriceField.onChange(event);

                    setBreakEvenPrice(
                      supplierCountry
                        ? (calculateBreakEvenPrice(
                            supplierCountry,
                            "NONE",
                            parseInt(
                              toDigitsOnly(event.target.value) || "0",
                              10,
                            ),
                          ) ?? 0)
                        : 0,
                    );
                  }}
                />
              </InputGroup>
              <FieldDescription>
                Break-even Price:
                {breakEvenPrice}
              </FieldDescription>
            </Field>
            <Field>
              <FieldLabel>Sale Price</FieldLabel>
              <FieldDescription>Price you want to sale for</FieldDescription>
              <InputGroup>
                <InputGroupAddon>VND</InputGroupAddon>
                <InputGroupInput
                  type="text"
                  inputMode="numeric"
                  pattern="[0-9]*"
                  placeholder="Sale price..."
                  {...baseSellingPriceField}
                  onKeyDown={handleDigitOnlyKeyDown}
                  onPaste={handleDigitOnlyPaste}
                  onChange={(event) => {
                    baseSellingPriceField.onChange(event);
                  }}
                />
              </InputGroup>
            </Field>
            <Field>
              <FieldLabel>SKU</FieldLabel>
              <FieldDescription>
                SKU for single-variant products
              </FieldDescription>
              <Input placeholder="SKU..." {...register("baseSku")} />
            </Field>
          </FieldGroup>
          <FieldGroup className="grid grid-cols-2">
            <Field className="col-span-1">
              <FieldLabel>Stock</FieldLabel>
              <Input
                type="text"
                inputMode="numeric"
                pattern="[0-9]*"
                placeholder="Stock..."
                {...baseStockField}
                onKeyDown={handleDigitOnlyKeyDown}
                onPaste={handleDigitOnlyPaste}
                onChange={(event) => {
                  baseStockField.onChange(event);
                }}
              />
            </Field>
          </FieldGroup>
        </>
      )}

      {isVariationEnable && (
        <div className="flex flex-col gap-4">
          <VariationFieldGroup variationState={variationState} />
          <VariationPriceTableField
            mode={mode}
            variations={variations}
            costCurrencyCode={costCurrencyCode}
            initialDraftSeeds={initialDraftSeeds}
            onVariantsChange={handleVariantsChange}
          />
        </div>
      )}
    </>
  );
};

export default PriceFieldGroup;
