# SOP-014 Die Change on Press Line P-12

Document: SOP-014 | Revision: C | Date: 2026-03-02 | Owner: Production Engineering | Equipment: hydraulic press HP-250

## 1 Purpose

This procedure describes a safe and repeatable die change on the HP-250 press on line P-12. It separates external setup (done while the previous order is still running) from internal setup (done while the press is stopped) to keep the die change time within the target of 20 minutes.

## 2 Scope and responsibilities

The procedure applies to all dies used on line P-12. The die change is carried out by a qualified setter. The shift supervisor releases the press for production after the first article inspection is passed.

## 3 Required tools and equipment

- Die cart DC-6 (rated load 8 t) or the roller conveyor of the press.
- Torque wrench 60–340 Nm with socket 36 mm.
- Two safety blocks for the slide (yellow, marked SB-P12).
- Personal padlock and tag for lockout/tagout.
- Die tag of the new die with the recorded die shut height.

## 4 External setup (before the press stops)

1. Check the next work order in MES and confirm the die number on the die tag.
2. Bring the new die on the die cart to the press and park it next to the roller conveyor.
3. Visually check the die: no cracks, no loose parts, guide pins clean and lubricated.
4. Prepare the blank material and the first article inspection sheet (WI-203).

## 5 Internal setup (press stopped)

1. Finish the last part of the running order and move the slide to top dead center.
2. Switch the press to setup mode with the key switch. Automatic feeding is now disabled.
3. Lower the slide in setup mode until the die is closed, then release the upper hydraulic clamps.
4. Raise the slide to top dead center and release the lower hydraulic clamps.
5. Push the old die out onto the roller conveyor and move it to the die cart.
6. Push the new die into the press and center it on the two locating pins (diameter 40 mm).
7. Lower the slide in setup mode at maximum 10 mm/s until it touches the upper die.
8. Clamp the upper and lower die. Hydraulic clamps must show 250 bar on the clamp gauge. For dies without a hydraulic clamping interface, use manual clamping bolts M24 and tighten them to 180 Nm in a cross pattern.
9. Enter the die shut height from the die tag on the HMI. Tolerance is ±0.1 mm.
10. Run one single stroke without a blank and check that no alarm is shown.
11. Produce the first part and perform the first article inspection according to WI-203.
12. After the inspection is passed, the shift supervisor releases the order in MES. Record the die change time.

## 6 Safety notes

Never reach between the die halves unless the slide rests on both safety blocks and lockout/tagout according to LOTO-001 is applied. The hold-to-run button in setup mode is not a safety device for working inside the die area.

If alarm E412 (die clamp pressure low) appears at any time, stop the die change and call maintenance. Do not run a stroke with unclamped or partially clamped dies.

## 7 Revision history

| Revision | Date | Change |
|---|---|---|
| A | 2024-05-10 | First release |
| B | 2025-02-18 | Added manual clamping for dies without hydraulic interface |
| C | 2026-03-02 | Separated external and internal setup; target time reduced to 20 minutes |
