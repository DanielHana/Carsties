"use client";
import { useParamsStore } from "@/hooks/useParamStore";
import React, { useState } from "react";
import { FaSearch } from "react-icons/fa";

export default function Search() {
  const setParams = useParamsStore((state) => state.setParams);
  const setSearchValue = useParamsStore((state) => state.setSearchValue);
  const searchValue = useParamsStore((state) => state.searchValue);

  const [value, setValue] = useState("");

  function onChange(event: any) {
    setSearchValue(event.target.value);
  }

  function search() {
    setParams({ searchTerm: searchValue });
  }

  return (
    <div className="flex w-100 items-center border-2 rounded-full py-2 shadow-sm">
      <input
        onChange={onChange}
        onKeyDown={(e: any) => {
          if (e.key === "Enter") {
            search();
          }
        }}
        value={searchValue}
        className="flex-grow pl-5 bg-transparent 
        focus:ring-0 text-sm text-gray-600
        focus:outline-none border-transparent focus:border:transparent"
        type="text"
        placeholder="Search for cars by make, model, or color"
      />
      <button onClick={search}>
        <FaSearch
          size={34}
          className="bg-red-400 text-white rounded-full p-2 cursor-pointer mx-2"
        />
      </button>
    </div>
  );
}
