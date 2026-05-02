import { Button } from "@/components/ui/button";
import {
  Field,
  FieldDescription,
  FieldGroup,
  FieldLabel,
} from "@/components/ui/field";
import { Plus } from "lucide-react";
import {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ChangeEvent,
} from "react";
import {
  useFormContext,
  useWatch,
} from "react-hook-form";
import {
  toDigitsOnly,
} from "./number-guard";
import {
  calculateBreakEvenPrice,
} from "./pricing/calculate-break-even-price";
import { COUNTRY_CONFIG } from "./pricing/config";
import {
  PriceFormInputData,
  PriceFormMode,
  PriceFormVariantInput,
} from "./types";
import VariationFieldGroup from "./variation-field-group/variation-field-group";
import VariationPriceTableField from "./variation-price-table-field/variation-price-table-field";
import { VariantDraftSeed } from "./variation-price-table-field/use-variant-draft-rows";
import { useVariation } from "./variation-field-group/use-variation";
import SingleVariantFields from "./single-variant-fields";
import {
  FORM_VALUE_UPDATE_OPTIONS,
  InitialVariationEditorState,
  mapEditorVariationsToFormInput,
  parseOptionalNonNegativeNumber,
  toInitialVariationState,
} from "./price-field-group.helpers";

type PriceFieldGroupProps = {
  mode: PriceFormMode;
};

/**
 * Orchestrates the product pricing section for both supported product modes:
 * - single variant products with one shared price/SKU/stock set
 * - variation products whose rows are generated from variation combinations
 *
 * This component owns the switching logic between those two modes and keeps the
 * local variation editor synchronized with React Hook Form.
 */
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
    const formVariations = getValues("variations") ?? [];
    const variants = getValues("variants") ?? [];

    return toInitialVariationState(
      mode,
      hasVariation,
      formVariations,
      variants,
    );
  }, [getValues, mode]);

  // `useVariation` is the client-side source of truth for the variation editor.
  // React Hook Form still stores the payload that will be submitted, so later
  // effects mirror this local state back into the form.
  const variationState = useVariation(initialVariationState.initialVariations);
  const { addVariation, isVariationEnable, variations } = variationState;
  const [initialDraftSeeds, setInitialDraftSeeds] = useState<
    VariantDraftSeed[]
  >(initialVariationState.initialDraftSeeds);
  const [breakEvenPrice, setBreakEvenPrice] = useState(0);
  const previousIsVariationEnableRef = useRef(isVariationEnable);
  const hasSyncedVariationsRef = useRef(false);

  const supplierCountry = useWatch({
    control,
    name: "supplierCountry",
  });
  const baseSellingPrice = useWatch({
    control,
    name: "baseSellingPrice",
  });

  /**
   * Clears the single-variant-only fields.
   *
   * We do this whenever variation mode becomes active because the generated
   * variant table becomes the new source of truth for price/SKU/stock.
   */
  const clearSingleVariantFields = useCallback(() => {
    setValue("baseSku", undefined, FORM_VALUE_UPDATE_OPTIONS);
    setValue("baseCostPrice", undefined, FORM_VALUE_UPDATE_OPTIONS);
    setValue("baseSellingPrice", undefined, FORM_VALUE_UPDATE_OPTIONS);
    setValue("baseStock", undefined, FORM_VALUE_UPDATE_OPTIONS);
  }, [setValue]);

  /**
   * Clears variation-specific form fields when the user switches back to a
   * single-variant product.
   */
  const resetVariationFields = useCallback(() => {
    setValue("variations", [], FORM_VALUE_UPDATE_OPTIONS);
    setValue("variants", [], FORM_VALUE_UPDATE_OPTIONS);
  }, [setValue]);

  const baseCostPriceField = register("baseCostPrice", {
    setValueAs: parseOptionalNonNegativeNumber,
  });
  const baseSellingPriceField = register("baseSellingPrice", {
    setValueAs: parseOptionalNonNegativeNumber,
  });
  const baseStockField = register("baseStock.quantity", {
    setValueAs: parseOptionalNonNegativeNumber,
  });
  const baseSkuField = register("baseSku");

  const handleBaseCostPriceChange = useCallback(
    (event: ChangeEvent<HTMLInputElement>) => {
      // Let RHF keep the raw field value in sync first, then derive the
      // break-even preview from the same numeric input.
      baseCostPriceField.onChange(event);

      const nextBreakEvenPrice = supplierCountry
        ? (calculateBreakEvenPrice(
            supplierCountry,
            "NONE",
            parseInt(toDigitsOnly(event.target.value) || "0", 10),
          ) ?? 0)
        : 0;

      setBreakEvenPrice(nextBreakEvenPrice);
    },
    [baseCostPriceField, supplierCountry],
  );

  const handleBaseSellingPriceChange = useCallback(
    (event: ChangeEvent<HTMLInputElement>) => {
      setValue(
        "baseSellingPrice",
        parseOptionalNonNegativeNumber(event.target.value),
        FORM_VALUE_UPDATE_OPTIONS,
      );
    },
    [setValue],
  );

  const handleVariantsChange = useCallback(
    (nextVariants: PriceFormVariantInput[]) => {
      // The generated variation table owns the `variants` form payload.
      // Every row edit writes the full current variant list back into RHF.
      setValue("variants", nextVariants, FORM_VALUE_UPDATE_OPTIONS);

      if (nextVariants.length > 0) {
        clearErrors("variants");
      }
    },
    [clearErrors, setValue],
  );

  const handleEnableVariation = useCallback(() => {
    // Reset hydration seeds so a fresh variation flow starts from empty editor
    // state rather than reusing rows from a previous configuration.
    setInitialDraftSeeds([]);
    clearErrors("variants");
    addVariation();
  }, [addVariation, clearErrors]);

  useEffect(() => {
    // If we cannot hydrate the variation editor from stored data, expose that
    // problem as a form-level validation error instead of failing silently.
    if (!initialVariationState.parseError) {
      return;
    }

    setError("variants", {
      type: "manual",
      message: initialVariationState.parseError,
    });
  }, [initialVariationState.parseError, setError]);

  useEffect(() => {
    // When variation mode toggles, keep the form payload consistent:
    // - enabling variations removes single-variant pricing fields
    // - disabling variations removes generated variation structures/rows
    const wasVariationEnabled = previousIsVariationEnableRef.current;
    previousIsVariationEnableRef.current = isVariationEnable;

    if (wasVariationEnabled === isVariationEnable) {
      return;
    }

    if (isVariationEnable) {
      setValue("hasVariation", true, FORM_VALUE_UPDATE_OPTIONS);
      clearSingleVariantFields();
      return;
    }

    setValue("hasVariation", false, FORM_VALUE_UPDATE_OPTIONS);
    resetVariationFields();
    clearSingleVariantFields();
  }, [
    clearSingleVariantFields,
    isVariationEnable,
    resetVariationFields,
    setValue,
  ]);

  useEffect(() => {
    // The first sync is initial hydration, so it should not mark the form as
    // dirty. Every later sync represents a real user edit in the variation
    // editor and should therefore become dirty and revalidate.
    const shouldDirty = hasSyncedVariationsRef.current;
    hasSyncedVariationsRef.current = true;

    setValue("variations", mapEditorVariationsToFormInput(variations), {
      shouldDirty,
      shouldValidate: isVariationEnable,
    });
  }, [isVariationEnable, setValue, variations]);

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
        <SingleVariantFields
          baseCostPriceField={baseCostPriceField}
          baseSellingPriceField={baseSellingPriceField}
          baseSkuField={baseSkuField}
          baseStockField={baseStockField}
          baseSellingPrice={baseSellingPrice}
          breakEvenPrice={breakEvenPrice}
          costCurrencyCode={costCurrencyCode}
          onBaseCostPriceChange={handleBaseCostPriceChange}
          onBaseSellingPriceChange={handleBaseSellingPriceChange}
        />
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
