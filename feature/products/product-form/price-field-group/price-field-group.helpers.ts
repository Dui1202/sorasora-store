import {
  PriceFormMode,
  PriceFormVariationInput,
  PriceFormVariantInput,
} from "./types";
import {
  parseExistingVariantsForEditor,
  parseExplicitVariationsForEditor,
} from "./variation-price-table-field/variation-price-table-field-utils";
import { VariantDraftSeed } from "./variation-price-table-field/use-variant-draft-rows";
import {
  createEmptyVariation,
  useVariation,
} from "./variation-field-group/use-variation";
import { toDigitsOnly } from "./number-guard";

/**
 * Data needed to bootstrap the variation editor UI.
 *
 * `initialVariations` drives the local variation form state while
 * `initialDraftSeeds` is passed to the generated variant table so existing
 * SKU/price/stock/image values can be restored in update mode.
 */
export type InitialVariationEditorState = {
  initialVariations: ReturnType<typeof useVariation>["variations"];
  initialDraftSeeds: VariantDraftSeed[];
  parseError: string | null;
};

/**
 * Shared React Hook Form update flags for programmatic field changes inside the
 * pricing flow. These updates should always mark the form dirty and re-run
 * validation because they affect the payload shape sent to the backend.
 */
export const FORM_VALUE_UPDATE_OPTIONS = {
  shouldDirty: true,
  shouldValidate: true,
} as const;

const DEFAULT_EDITOR_STATE: InitialVariationEditorState = {
  initialVariations: [],
  initialDraftSeeds: [],
  parseError: null,
};

/**
 * Converts numeric text input into the optional number shape used by the form.
 * Empty input stays `undefined` so the zod schema can still distinguish
 * "missing" from an actual numeric value.
 */
export function parseOptionalNonNegativeNumber(value: unknown) {
  const digits = toDigitsOnly(String(value ?? ""));
  return digits === "" ? undefined : Number(digits);
}

/**
 * The local variation editor keeps order implicitly by array position.
 * This helper turns that client-side shape into the explicit form contract
 * expected by the backend by re-attaching `position` to each variation and
 * option before writing it into React Hook Form state.
 */
export function mapEditorVariationsToFormInput(
  variations: ReturnType<typeof useVariation>["variations"],
): PriceFormVariationInput[] {
  return variations.map((variation, variationIndex) => ({
    id: variation.id,
    name: variation.name,
    position: variationIndex,
    options: variation.optionsList.map((option, optionIndex) => ({
      id: option.id,
      value: option.value,
      position: optionIndex,
    })),
  }));
}

/**
 * Creates the initial state for the variation editor.
 *
 * Update mode supports two sources of truth:
 * 1. Explicit variation relations from the new schema
 * 2. Legacy products whose variation structure only exists in `variant.name`
 *
 * We always prefer the explicit structure and only parse names as a fallback
 * for older records that have not been rewritten yet.
 */
export function toInitialVariationState(
  mode: PriceFormMode,
  hasVariation: boolean,
  formVariations: PriceFormVariationInput[],
  variants: PriceFormVariantInput[],
): InitialVariationEditorState {
  if (mode !== "update" || !hasVariation) {
    return DEFAULT_EDITOR_STATE;
  }

  const parsed =
    formVariations.length > 0
      ? parseExplicitVariationsForEditor(formVariations, variants)
      : parseExistingVariantsForEditor(variants);

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
}
