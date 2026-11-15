"""Run from the project root: python Tools/DungeonRoom/segment_dungeon.py source.png"""
import argparse
import json
from pathlib import Path
from PIL import Image, ImageDraw, ImageFilter

parser = argparse.ArgumentParser()
parser.add_argument("source", type=Path)
parser.add_argument("--output", type=Path, default=Path("Assets/Art/Map/Dungeon/LayeredShrine"))
args = parser.parse_args()
out = args.output
(out / "Props").mkdir(parents=True, exist_ok=True)
W, H = 1606, 1075
src = Image.open(args.source).convert("RGBA").resize((W, H), Image.Resampling.LANCZOS)
# Replace only bright white outline pixels using neighbouring artwork.
original = src.copy()
for y in range(602, 643):
    for x in range(1097, 1138):
        r, g, b, _ = original.getpixel((x, y))
        if min(r, g, b) > 220 and max(r,g,b)-min(r,g,b)<23 and (abs(x-1100)<2 or abs(x-1134)<2 or abs(y-605)<2 or abs(y-639)<2):
            src.putpixel((x,y), original.getpixel((x+3 if abs(x-1100)<2 or abs(x-1134)<2 else x,y+3 if abs(y-605)<2 or abs(y-639)<2 else y)))
# Work in layout pixels, then export all registered layers at double density.
density = 2
src = src.resize((W*density, H*density), Image.Resampling.LANCZOS)
src = src.filter(ImageFilter.UnsharpMask(radius=1.8, percent=115, threshold=3))
src.save(out / "Map_Base.png")
fg = Image.new("RGBA",src.size)
top = fg.copy()
props, paths, zones = [], [], []

def points(spec):
    return [tuple(map(int,p.split(","))) for p in spec.split()]

def cut(name, spec, target=fg, shift=(0,0)):
    poly = points(spec)
    mask = Image.new("L",(W,H))
    ImageDraw.Draw(mask).polygon(poly,fill=255)
    if name == "ArenaArch":
        # Only the overhead stones can cover the player unconditionally.
        # The long vertical pier below the arch follows the adjoining wall.
        lower = mask.crop((0,418,W,H))
        pier = Image.new("L",(W,H))
        pier.paste(lower,(0,418))
        export_mask("ArenaGatePier",pier,fg,(0,0))
        ImageDraw.Draw(mask).rectangle((0,418,W,H),fill=0)
    export_mask(name,mask,target,shift)

def export_mask(name,mask,target,shift):
    rect = mask.getbbox()
    sprite = src.crop(tuple(v*density for v in rect))
    sprite.putalpha(mask.crop(rect).resize(sprite.size, Image.Resampling.BILINEAR))
    x,y = rect[0]+shift[0], rect[1]+shift[1]
    target.alpha_composite(sprite,(x*density,y*density))
    if target is fg:
        # A long parapet changes ground depth along its length. Give each
        # short stretch its own ground contact instead of using the far end.
        vertical = "Stair" in name or name == "ArenaGatePier"
        horizontal = name.startswith(("Bridge", "ArenaWestRail", "ArenaEastRail"))
        length = sprite.height if vertical else sprite.width
        stride = 20*density if vertical or horizontal else length
        for start in range(0,length,stride):
            end = min(start+stride,length)
            box = (0,start,sprite.width,end) if vertical else (start,0,end,sprite.height)
            segment = sprite.crop(box)
            bounds = segment.getchannel("A").getbbox()
            if not bounds:
                continue
            segment = segment.crop(bounds)
            key = name if start == 0 else f"{name}_{start//stride:02d}"
            segment.save(out / "Props" / (key+".png"))
            props.append(dict(name=key,x=x+(box[0]+bounds[0]+segment.width/2)/density,
                              y=y+(box[1]+bounds[3])/density))

brazier = "1041,333 1046,323 1048,301 1039,289 1043,280 1049,282 1051,267 1057,276 1066,249 1064,271 1072,262 1072,277 1080,273 1078,283 1087,281 1088,291 1081,303 1077,325 1086,329 1083,337 1061,346"
cut("BrazierNW",brazier)
cut("BrazierNE",brazier,shift=(441,8))
cut("BrazierSW","1077,690 1086,682 1089,662 1079,648 1080,634 1087,638 1086,617 1096,630 1101,603 1110,579 1108,611 1115,608 1117,630 1125,619 1124,634 1131,632 1131,646 1122,659 1120,681 1131,685 1129,693 1107,702")
cut("BrazierSE","1466,682 1476,674 1479,654 1469,641 1470,625 1477,629 1478,613 1485,620 1493,585 1503,570 1498,606 1508,598 1507,619 1517,610 1515,625 1522,622 1522,639 1514,651 1511,671 1522,678 1519,688 1495,696")
for name,x,y,w,h in [("WestColumn",615,354,40,130),("EastColumn",851,353,39,132),("FrontColumn",574,435,38,124),("BrokenColumn",648,315,28,45)]:
    profile=[(-.48,.05),(-.22,0),(.32,.01),(.5,.06),(.4,.13),(.24,.17),(.23,.82),(.47,.87),(.49,.96),(.15,1),(-.47,.96),(-.48,.88),(-.26,.82),(-.24,.17),(-.43,.14)]
    cut(name," ".join(f"{round(x+a*w)},{round(y-h+b*h)}" for a,b in profile))
cut("BridgeParapet","435,551 453,548 460,559 474,554 491,551 522,549 521,540 540,538 545,548 607,546 620,547 633,546 677,546 678,523 695,520 704,530 708,530 706,571 685,585 670,574 623,582 601,604 588,580 551,580 531,603 518,583 479,588 451,607 439,632 425,632")
cut("BridgeArchRail","700,527 734,510 790,499 803,507 802,539 776,562 770,546 755,536 732,540 710,562 701,585")
cut("ArenaWestRail","979,647 999,650 1014,672 1049,688 1088,702 1129,716 1177,724 1187,738 1175,783 1148,771 1145,749 1106,739 1070,732 1000,701")
cut("ArenaEastRail","1351,750 1383,741 1397,731 1450,714 1493,698 1542,667 1556,651 1560,582 1582,565 1596,575 1582,686 1551,716 1495,746 1406,776 1381,785 1376,818 1349,824")
for name,spec in [
("LowerStairWest","175,644 193,642 195,664 181,843 186,852 185,876 163,877 162,844"),
("LowerStairEast","341,641 361,637 367,654 381,818 390,838 391,874 368,875 362,850"),
("UpperStairWest","167,297 188,297 185,319 177,423 157,427 160,391"),
("UpperStairEast","277,295 297,291 298,325 309,418 288,425"),
("BridgeNorthRail","424,485 600,480 608,469 624,465 632,501 612,508 606,499 427,510")]:
    cut(name,spec)
cut("ArenaArch","886,325 901,321 914,327 927,328 939,340 956,362 958,374 974,401 980,426 989,454 998,481 991,501 990,609 999,645 985,659 955,646 950,608 948,495 951,459 948,431 943,409 935,389 928,380 918,379 912,383 908,395 907,403 898,401 901,379 905,369 891,352",top)
cut("PortalCrown","12,834 22,803 52,771 71,757 82,738 101,727 119,733 125,749 151,766 166,789 161,834 151,839 145,805 128,788 122,813 112,793 104,823 92,795 76,789 67,815 61,797 46,813 34,845 20,854",top)
fg.save(out/"Map_Occluders.png")
top.save(out/"Map_Overhangs.png")
def edge(name,spec):
    paths.append(dict(name=name,points=[dict(x=x,y=y) for x,y in points(spec)]))
edge("WestCliffs","112,903 78,929 72,958 100,1009 164,1049 422,1055 496,1016 451,952 440,896 386,864 359,855 345,664 395,617 420,574 446,561 686,553 734,517 807,506 842,536 941,524 947,471")
edge("WestUpperBoundary","137,881 164,862 179,664 122,642 59,602 27,565 29,499 58,457 122,427 169,418 177,309 109,284 52,251 46,217 88,165 157,137 209,93 285,113 320,156 357,185 367,224 340,264 286,307 294,418 351,419 393,450 412,493 625,485 616,465 557,432 531,397 527,355 475,328 463,308 511,278 599,255 671,255 684,275 777,276 779,255 847,263 891,301 901,379")
edge("ArenaNorth","936,390 978,356 1026,310 1114,279 1194,261 1204,244 1264,224 1354,224 1408,261 1470,285 1535,328 1573,380 1585,455")
edge("ArenaSouth","948,468 978,502 986,615 1016,665 1100,706 1178,735 1214,769 1267,801 1344,779 1365,747 1461,713 1542,664 1570,614 1585,455")
edge("ArenaGateNorthJamb","901,379 914,370 936,390")
edge("ArenaGateSouthJamb","947,471 948,468")
edge("PortalRearLimit","112,903 101,897 117,877 137,881")
for p in props[:8]:
    x,y=round(p["x"]),round(p["y"])-5
    edge(p["name"]+"Footprint",f"{x-10},{y-3} {x},{y-8} {x+10},{y-3} {x+10},{y+1} {x},{y+5} {x-10},{y+1} {x-10},{y-3}")
def zone(name,x,y,r,light=""):
    zones.append(dict(name=name,x=x,y=y,radius=r,light=light))
zone("PortalSpawnExit",122,893,23)
zone("ShrineInteract",731,294,25)
zone("BossRootSteps",1301,277,45)
for p in props[:4]: zone(p["name"],p["x"],p["y"]-14,20,"green")
for i,(x,y) in enumerate([(60,521),(380,487),(176,824),(378,824),(550,246),(681,217),(768,218),(817,254)]):
    zone("Torch"+str(i+1),x,y,16,"orange")
(out/"Layout.json").write_text(json.dumps(dict(width=W,height=H,ppu=26,props=props,paths=paths,zones=zones),indent=2))

