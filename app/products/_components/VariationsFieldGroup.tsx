"use client";

import { Field, FieldGroup, FieldLabel } from "@/components/ui/field";
import { Button } from "@/components/ui/button";
import { Plus } from "lucide-react";
import React, { useState } from "react";

const VariationsFieldGroup = () => {
  const [isVariationEnable, setVariationEnable] = useState(false);
  const onEnableVariationButtonClick = () => {
    setVariationEnable(true);
  };
  return (
    <FieldGroup>
      <Field>
        <FieldLabel>Variations</FieldLabel>
        {!isVariationEnable && (
          <div>
            <Button onClick={onEnableVariationButtonClick}>
              <Plus />
              Enable Variation
            </Button>
          </div>
        )}
      </Field>
    </FieldGroup>
  );
};

export default VariationsFieldGroup;
