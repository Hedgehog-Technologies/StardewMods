const core = require("@actions/core");

const tag = process.env.tag;
const splitTag = tag.split('/');

console.log(tag);
console.log(splitTag);

core.exportVariable('tag_project', splitTag[0]);
core.exportVariable('tag_version', splitTag[1]);
